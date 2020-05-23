using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Reflection;
using ECommon.Socketing;
using ENode.Commanding;
using ENode.Configurations;
using ENode.Domain;
using ENode.Eventing;
using ENode.Messaging;
using ENode.RabbitMQ;
using RabbitMQTopic;

namespace BankTransferSample
{
    public static class ENodeExtensions
    {
        private static CommandService _commandService;
        private static CommandConsumer _commandConsumer;
        private static ApplicationMessagePublisher _applicationMessagePublisher;
        private static ApplicationMessageConsumer _applicationMessageConsumer;
        private static DomainEventPublisher _domainEventPublisher;
        private static DomainEventConsumer _eventConsumer;
        private static DomainExceptionPublisher _exceptionPublisher;
        private static DomainExceptionConsumer _exceptionConsumer;

        public static ENodeConfiguration BuildContainer(this ENodeConfiguration enodeConfiguration)
        {
            enodeConfiguration.GetCommonConfiguration().BuildContainer();
            return enodeConfiguration;
        }

        public static ENodeConfiguration UseRabbitMQ(this ENodeConfiguration enodeConfiguration)
        {
            var assemblies = new[] { Assembly.GetExecutingAssembly() };
            enodeConfiguration.RegisterTopicProviders(assemblies);

            var configuration = enodeConfiguration.GetCommonConfiguration();

            _commandService = new CommandService();
            _applicationMessagePublisher = new ApplicationMessagePublisher();
            _domainEventPublisher = new DomainEventPublisher();
            _exceptionPublisher = new DomainExceptionPublisher();

            configuration.SetDefault<ICommandService, CommandService>(_commandService);
            configuration.SetDefault<IMessagePublisher<IApplicationMessage>, ApplicationMessagePublisher>(_applicationMessagePublisher);
            configuration.SetDefault<IMessagePublisher<DomainEventStreamMessage>, DomainEventPublisher>(_domainEventPublisher);
            configuration.SetDefault<IMessagePublisher<IDomainException>, DomainExceptionPublisher>(_exceptionPublisher);

            return enodeConfiguration;
        }

        public static ENodeConfiguration StartRabbitMQ(this ENodeConfiguration enodeConfiguration)
        {
            var amqpUri = new Uri("amqp://demo:123456@localhost/test");
            var clientName = "BankTransferSample";
            _commandService.InitializeRabbitMQ(new ProducerSettings
            {
                AmqpUri = amqpUri,
                ClientName = clientName
            },
            new string[] { Constants.CommandTopic },
            new CommandResultProcessor().Initialize(new IPEndPoint(SocketUtils.GetLocalIPV4(), 9000)));
            _applicationMessagePublisher.InitializeRabbitMQ(new ProducerSettings
            {
                AmqpUri = amqpUri,
                ClientName = clientName
            },
            new string[] { Constants.ApplicationMessageTopic });
            _domainEventPublisher.InitializeRabbitMQ(new ProducerSettings
            {
                AmqpUri = amqpUri,
                ClientName = clientName
            },
            new string[] { Constants.EventTopic });
            _exceptionPublisher.InitializeRabbitMQ(new ProducerSettings
            {
                AmqpUri = amqpUri,
                ClientName = clientName
            },
            new string[] { Constants.ExceptionTopic });

            _commandConsumer = new CommandConsumer().InitializeRabbitMQ(new ConsumerSettings
            {
                AmqpUri = amqpUri,
                ClientName = clientName,
                Mode = ConsumeMode.Pull,
                PrefetchCount = (ushort)ENode.Configurations.ENodeConfiguration.Instance.Setting.EventMailBoxPersistenceMaxBatchSize
            }).Subscribe(Constants.CommandTopic);
            _applicationMessageConsumer = new ApplicationMessageConsumer().InitializeRabbitMQ(new ConsumerSettings
            {
                AmqpUri = amqpUri,
                ClientName = clientName,
                Mode = ConsumeMode.Pull,
                PrefetchCount = (ushort)ENode.Configurations.ENodeConfiguration.Instance.Setting.EventMailBoxPersistenceMaxBatchSize
            }).Subscribe(Constants.ApplicationMessageTopic);
            _eventConsumer = new DomainEventConsumer().InitializeRabbitMQ(new ConsumerSettings
            {
                AmqpUri = amqpUri,
                ClientName = clientName,
                Mode = ConsumeMode.Pull,
                PrefetchCount = (ushort)ENode.Configurations.ENodeConfiguration.Instance.Setting.EventMailBoxPersistenceMaxBatchSize
            }).Subscribe(Constants.EventTopic);
            _exceptionConsumer = new DomainExceptionConsumer().InitializeRabbitMQ(new ConsumerSettings
            {
                AmqpUri = amqpUri,
                ClientName = clientName,
                Mode = ConsumeMode.Pull,
                PrefetchCount = (ushort)ENode.Configurations.ENodeConfiguration.Instance.Setting.EventMailBoxPersistenceMaxBatchSize
            }).Subscribe(Constants.ExceptionTopic);

            _exceptionConsumer.Start();
            _eventConsumer.Start();
            _applicationMessageConsumer.Start();
            _commandConsumer.Start();
            _applicationMessagePublisher.Start();
            _domainEventPublisher.Start();
            _exceptionPublisher.Start();
            _commandService.Start();

            return enodeConfiguration;
        }

        public static ENodeConfiguration ShutdownRabbitMQ(this ENodeConfiguration enodeConfiguration)
        {
            _commandService.Shutdown();
            _applicationMessagePublisher.Shutdown();
            _domainEventPublisher.Shutdown();
            _exceptionPublisher.Shutdown();
            _commandConsumer.Shutdown();
            _applicationMessageConsumer.Shutdown();
            _eventConsumer.Shutdown();
            _exceptionConsumer.Shutdown();
            return enodeConfiguration;
        }
    }
}
