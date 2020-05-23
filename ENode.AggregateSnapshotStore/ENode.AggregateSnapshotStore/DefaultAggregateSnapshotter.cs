using AggregateSnapshotStore;
using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace ENode.AggregateSnapshotStore
{
    /// <summary>
    /// 聚合仓储基类，用于快照获取和生成
    /// </summary>
    public class DefaultAggregateSnapshotter : IAggregateSnapshotter
    {
        private readonly IEventStore _eventStore;
        private readonly ITypeNameProvider _typeNameProvider;
        private readonly IAggregateSnapshotStore _snapshotStore;
        private readonly BinaryFormatter _binaryFormatter;

        /// <summary>
        /// 聚合仓储基类，用于快照获取和生成
        /// </summary>
        /// <param name="eventStore"></param>
        /// <param name="typeNameProvider"></param>
        /// <param name="snapshotStore"></param>
        public DefaultAggregateSnapshotter(
           IEventStore eventStore,
           ITypeNameProvider typeNameProvider,
           IAggregateSnapshotStore snapshotStore)
        {
            _eventStore = eventStore;
            _typeNameProvider = typeNameProvider;
            _binaryFormatter = new BinaryFormatter();
            _snapshotStore = snapshotStore;
        }

        /// <summary>
        /// 从快照中还原
        /// </summary>
        /// <param name="aggregateRootType"></param>
        /// <param name="aggregateRootId"></param>
        /// <returns></returns>
        public async Task<IAggregateRoot> RestoreFromSnapshotAsync(Type aggregateRootType, string aggregateRootId)
        {
            try
            {
                var aggregateRootTypeName = _typeNameProvider.GetTypeName(aggregateRootType);
                var snapshotData = await _snapshotStore.FindLatestAsync(aggregateRootId, aggregateRootTypeName);
                if (snapshotData == null || snapshotData.Data == null || snapshotData.Data.Length == 0)
                {
                    return null;
                }
                IAggregateRoot aggregateRoot;
                using (var ms = new MemoryStream())
                {
                    ms.Write(snapshotData.Data, 0, snapshotData.Data.Length);
                    ms.Position = 0;
                    aggregateRoot = _binaryFormatter.Deserialize(ms) as IAggregateRoot;
                }
                if (aggregateRoot == null)
                {
                    return null;
                }
                var eventStreams = await _eventStore.QueryAggregateEventsAsync(aggregateRootId, aggregateRootTypeName, aggregateRoot.Version + 1, int.MaxValue).ConfigureAwait(false);
                if (eventStreams != null && eventStreams.Any())
                {
                    aggregateRoot.ReplayEvents(eventStreams);
                }
                return aggregateRoot;
            }
            catch
            {
                return null;
            }
        }
    }
}
