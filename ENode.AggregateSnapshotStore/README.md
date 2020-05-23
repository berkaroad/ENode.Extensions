# ENode.AggregateSnapshotStore
AggregateSnapshotStore adapter for enode.

The default implementation of `IAggregateSnapshotRequestQueue` is `DefaultAggregateSnapshotRequestProcessor`, that will take snapshot in single thread in ProcessorHost.

If you want to deploy snapshot app standly, you can implement `IAggregateSnapshotRequestQueue` to enqueue request to mq, and then subscribe from mq and take snapshot in snapshot app.

Reference [AggregateSnapshotStore](https://github.com/berkaroad/AggregateSnapshotStore)


## Installation and Usage

In InventoryControl.ProcessorHost project, install these packages:

```
dotnet add package ENode.AggregateSnapshotStore
dotnet add package AggregateSnapshotStore.SqlServer

```

```csharp
// ENodeExtensions.cs
using AggregateSnapshotStore;
using AggregateSnapshotStore.SqlServer;
using ECommon.Components;
using ENode.AggregateSnapshotStore;
using ENode.Configurations;

namespace InventoryControl.ProcessorHost
{
    public static class ENodeExtensions
    {
        public static ENodeConfiguration UseSqlServerAggregateSnapshotStore(this ENodeConfiguration enodeConfiguration)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();
            enodeConfiguration.UseAggregateSnapshotStore<SqlServerAggregateSnapshotStore>();
            configuration.SetDefault<IAggregateSnapshotRequestFilter, DefaultAggregateSnapshotRequestFilter>((string)null);
            configuration.SetDefault<IAggregateSnapshotRequestQueue, DefaultAggregateSnapshotRequestProcessor>((string)null);
            return enodeConfiguration;
        }

        public static ENodeConfiguration InitializeSqlServerAggregateSnapshotStore(this ENodeConfiguration enodeConfiguration, string connectionString)
        {
            enodeConfiguration.InitializeAggregateSnapshotStore<SqlServerAggregateSnapshotStore>(s =>
            s.Initialize(
                connectionString
            ));
            ((DefaultAggregateSnapshotRequestProcessor)ObjectContainer.Resolve<IAggregateSnapshotRequestQueue>()).Initialize(
                TimeSpan.FromSeconds(3),
                ObjectContainer.Resolve<IAggregateSnapshotRequestFilter>(),
                ObjectContainer.Resolve<IAggregateSnapshotSaver>()
            );
            ((DefaultAggregateSnapshotRequestFilter)ObjectContainer.Resolve<IAggregateSnapshotRequestFilter>()).Initialize(
                20,
                ObjectContainer.Resolve<IAggregateSnapshotStore>()
            );
            return enodeConfiguration;
        }

        public static ENodeConfiguration StartAggregateSnapshotRequestProcessor(this ENodeConfiguration enodeConfiguration)
        {
            ((DefaultAggregateSnapshotRequestProcessor)ObjectContainer.Resolve<IAggregateSnapshotRequestQueue>()).Start();
            return enodeConfiguration;
        }

        public static ENodeConfiguration ShutdownAggregateSnapshotRequestProcessor(this ENodeConfiguration enodeConfiguration)
        {
            ((DefaultAggregateSnapshotRequestProcessor)ObjectContainer.Resolve<IAggregateSnapshotRequestQueue>()).Stop();
            return enodeConfiguration;
        }
    }
}

// Bootstrap.cs
using System;
using System.Reflection;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.Logging;
using ENode.AggregateSnapshotStore;
using ENode.Configurations;
using ENode.SqlServer;
using InventoryControl.Common;
using Microsoft.Extensions.Configuration;
using ECommonConfiguration = ECommon.Configurations.Configuration;

namespace InventoryControl.ProcessorHost
{
    public class Bootstrap
    {
        private static IConfiguration _config;
        private static ILogger _logger;
        private static ECommonConfiguration _ecommonConfiguration;
        private static ENodeConfiguration _enodeConfiguration;

        public static void Initialize(IConfiguration config)
        {
            _config = config;
            InventoryControl.Common.ConfigSettings.Initialize(config);
            InitializeECommon();
            InitializeENode();
        }
        public static void Start()
        {
            try
            {
                _enodeConfiguration.StartRabbitMQ().Start();
            }
            catch (Exception ex)
            {
                _logger.Error("RabbitMQ start failed.", ex);
                throw;
            }
            _enodeConfiguration.StartAggregateSnapshotRequestProcessor();
        }

        public static void Stop()
        {
            try
            {
                _enodeConfiguration.ShutdownRabbitMQ().Stop();
            }
            catch (Exception ex)
            {
                _logger.Error("RabbitMQ stop failed.", ex);
                throw;
            }
            _enodeConfiguration.ShutdownAggregateSnapshotRequestProcessor();
        }

        private static void InitializeECommon()
        {
            _ecommonConfiguration = ECommonConfiguration
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net()
                .UseJsonNet()
                .RegisterUnhandledExceptionHandler();
        }
        private static void InitializeENode()
        {
            var assemblies = new[]
            {
                Assembly.Load("InventoryControl.Common"),
                Assembly.Load("InventoryControl.Domain"),
                Assembly.Load("InventoryControl.Commands"),
                Assembly.Load("InventoryControl.CommandHandlers"),
                Assembly.Load("InventoryControl.Messages"),
                Assembly.Load("InventoryControl.MessagePublishers"),
                Assembly.Load("InventoryControl.ReadModel"),
                Assembly.Load("InventoryControl.Repositories"),
                Assembly.Load("InventoryControl.Repositories.Sal"),
                Assembly.Load("InventoryControl.ProcessManagers"),
                Assembly.Load("InventoryControl.Snapshots"),
                Assembly.GetExecutingAssembly()
            };

            var connectionString = ConfigSettings.ENodeConnectionString;

            _enodeConfiguration = _ecommonConfiguration
                .CreateENode()
                .RegisterENodeComponents()
                .RegisterBusinessComponents(assemblies)
                .UseSqlServerLockService()
                .UseSqlServerEventStore()
                .UseSqlServerPublishedVersionStore()
                .UseSqlServerAggregateSnapshotStore()
                .UseRabbitMQ()
                .BuildContainer()
                .InitializeSqlServerEventStore(connectionString)
                .InitializeSqlServerPublishedVersionStore(connectionString)
                .InitializeSqlServerAggregateSnapshotStore(ConfigSettings.SnapshotConnectionString)
                .InitializeSqlServerLockService(connectionString)
                .InitializeBusinessAssemblies(assemblies);

            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(Bootstrap).FullName);
            _logger.Info("ENode initialized.");
        }
    }
}
```

In InventoryControl.Snapshots project, install these packages:

```
dotnet add package ENode
dotnet add package AggregateSnapshotStore
```

```csharp
// StockBoxSnapshotter.cs
using AggregateSnapshotStore;
using ECommon.Components;
using ENode.Messaging;
using InventoryControl.Domain.StockModule.Events;
using System.Threading.Tasks;

namespace InventoryControl.Snapshots.StockModule
{
    [Component]
    public class StockBoxSnapshotter
        : IMessageHandler<FreezeStockBoxStockCommitted>
        , IMessageHandler<FreezeStockBoxStockRolledBack>
        , IMessageHandler<CastStockBoxDowningCommitted>
        , IMessageHandler<CastStockBoxDowningRolledBack>
    {
        private IAggregateSnapshotRequestQueue _takeSnapshotRequestQueue;

        public StockBoxSnapshotter(IAggregateSnapshotRequestQueue takeSnapshotRequestQueue)
        {
            _takeSnapshotRequestQueue = takeSnapshotRequestQueue;
        }

        public Task HandleAsync(FreezeStockBoxStockCommitted message)
        {
            var headerInfo = new AggregateSnapshotHeader(message.AggregateRootStringId, message.AggregateRootTypeName, message.Version);
            _takeSnapshotRequestQueue.Enqueue(headerInfo);
            return Task.CompletedTask;
        }

        public Task HandleAsync(FreezeStockBoxStockRolledBack message)
        {
            var headerInfo = new AggregateSnapshotHeader(message.AggregateRootStringId, message.AggregateRootTypeName, message.Version);
            _takeSnapshotRequestQueue.Enqueue(headerInfo);
            return Task.CompletedTask;
        }

        public Task HandleAsync(CastStockBoxDowningCommitted message)
        {
            var headerInfo = new AggregateSnapshotHeader(message.AggregateRootStringId, message.AggregateRootTypeName, message.Version);
            _takeSnapshotRequestQueue.Enqueue(headerInfo);
            return Task.CompletedTask;
        }

        public Task HandleAsync(CastStockBoxDowningRolledBack message)
        {
            var headerInfo = new AggregateSnapshotHeader(message.AggregateRootStringId, message.AggregateRootTypeName, message.Version);
            _takeSnapshotRequestQueue.Enqueue(headerInfo);
            return Task.CompletedTask;
        }
    }
}

```

## History Publish

### 1.0.1

Upgrade package `AggregateSnapshotStore` to 1.0.1

### 1.0.0

init