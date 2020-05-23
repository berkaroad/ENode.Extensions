using AggregateSnapshotStore;
using ECommon.Components;
using ECommon.Logging;
using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace ENode.AggregateSnapshotStore
{
    /// <summary>
    /// Aggregate snapshot saver
    /// </summary>
    public class DefaultAggregateSnapshotSaver : IAggregateSnapshotSaver
    {
        private readonly ITypeNameProvider _typeNameProvider;
        private readonly IAggregateRootFactory _aggregateRootFactory;
        private readonly IEventStore _eventStore;
        private readonly IAggregateSnapshotStore _snapshotStore;
        private readonly BinaryFormatter _binaryFormatter;
        private readonly ILogger _logger;
        private int _batchSaveSize;

        /// <summary>
        /// Aggregate snapshot saver
        /// </summary>
        /// <param name="typeNameProvider"></param>
        /// <param name="aggregateRootFactory"></param>
        /// <param name="eventStore"></param>
        /// <param name="snapshotStore"></param>
        public DefaultAggregateSnapshotSaver(
            ITypeNameProvider typeNameProvider,
            IAggregateRootFactory aggregateRootFactory,
            IEventStore eventStore,
            IAggregateSnapshotStore snapshotStore)
        {
            _typeNameProvider = typeNameProvider;
            _aggregateRootFactory = aggregateRootFactory;
            _eventStore = eventStore;
            _snapshotStore = snapshotStore;
            _binaryFormatter = new BinaryFormatter();
            _batchSaveSize = 100;
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
        }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="batchSaveSize"></param>
        public void Initialize(int batchSaveSize)
        {
            _batchSaveSize = batchSaveSize;
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <param name="snapshotHeaders"></param>
        /// <returns></returns>
        public async Task SaveAsync(IEnumerable<AggregateSnapshotHeader> snapshotHeaders)
        {
            var queue = new ConcurrentQueue<AggregateSnapshotData>();
            foreach (var snapshotHeader in snapshotHeaders)
            {
                try
                {
                    var aggregateRootType = _typeNameProvider.GetType(snapshotHeader.AggregateRootTypeName);
                    IAggregateRoot aggregateRoot = null;
                    IEnumerable<DomainEventStream> eventStreams = null;
                    var lastSnapshotData = await _snapshotStore.FindLatestAsync(snapshotHeader.AggregateRootId, snapshotHeader.AggregateRootTypeName);
                    if (lastSnapshotData != null)
                    {
                        try
                        {
                            using (var ms = new MemoryStream())
                            {
                                await ms.WriteAsync(lastSnapshotData.Data, 0, lastSnapshotData.Data.Length);
                                await ms.FlushAsync();
                                ms.Position = 0;
                                aggregateRoot = _binaryFormatter.Deserialize(ms) as IAggregateRoot;
                            }
                        }
                        catch
                        {
                            aggregateRoot = null;
                        }
                    }
                    if (aggregateRoot == null)
                    {
                        // 无快照
                        aggregateRoot = _aggregateRootFactory.CreateAggregateRoot(aggregateRootType);
                        eventStreams = await _eventStore.QueryAggregateEventsAsync(snapshotHeader.AggregateRootId, snapshotHeader.AggregateRootTypeName, 1, snapshotHeader.Version);
                    }
                    else
                    {
                        eventStreams = await _eventStore.QueryAggregateEventsAsync(snapshotHeader.AggregateRootId, snapshotHeader.AggregateRootTypeName, lastSnapshotData.Version + 1, snapshotHeader.Version);
                    }
                    if (eventStreams != null && eventStreams.Any())
                    {
                        aggregateRoot.ReplayEvents(eventStreams);
                        using (var ms = new MemoryStream())
                        {
                            _binaryFormatter.Serialize(ms, aggregateRoot);
                            await ms.FlushAsync();
                            ms.Position = 0;
                            byte[] buffer = new byte[ms.Length];
                            await ms.ReadAsync(buffer, 0, buffer.Length);
                            queue.Enqueue(new AggregateSnapshotData(snapshotHeader.AggregateRootId, snapshotHeader.AggregateRootTypeName, aggregateRoot.Version, buffer));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Save snapshot fail:{ex.Message}. AggregateRootId={snapshotHeader.AggregateRootId},AggregateRootTypeName={snapshotHeader.AggregateRootTypeName}", ex);
                }
            }

            var snapshotDataList = new List<AggregateSnapshotData>();
            while (queue.TryDequeue(out AggregateSnapshotData snapshotData))
            {
                snapshotDataList.Add(snapshotData);
                if (snapshotDataList.Count == _batchSaveSize)
                {
                    try
                    {
                        await _snapshotStore.BatchSaveAsync(snapshotDataList);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Save snapshot fail:{ex.Message}.", ex);
                    }
                    finally
                    {
                        snapshotDataList.Clear();
                    }
                }
            }
        }
    }
}
