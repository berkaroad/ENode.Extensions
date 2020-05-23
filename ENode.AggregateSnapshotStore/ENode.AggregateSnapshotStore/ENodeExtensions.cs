using AggregateSnapshotStore;
using ECommon.Components;
using ENode.Configurations;
using ENode.Domain;
using System;

namespace ENode.AggregateSnapshotStore
{
    /// <summary>
    /// AggregateSnapshotStore enode extensions
    /// </summary>
    public static class ENodeExtensions
    {
        /// <summary>
        /// Use AggregateSnapshotStore
        /// </summary>
        /// <typeparam name="TAggregateSnapshotStore"></typeparam>
        /// <param name="enodeConfiguration"></param>
        /// <returns></returns>
        public static ENodeConfiguration UseAggregateSnapshotStore<TAggregateSnapshotStore>(this ENodeConfiguration enodeConfiguration)
            where TAggregateSnapshotStore : class, IAggregateSnapshotStore
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();
            configuration.SetDefault<IAggregateSnapshotStore, TAggregateSnapshotStore>((string)null);
            configuration.SetDefault<IAggregateSnapshotSaver, DefaultAggregateSnapshotSaver>((string)null);
            configuration.SetDefault<IAggregateSnapshotter, DefaultAggregateSnapshotter>((string)null);
            return enodeConfiguration;
        }

        /// <summary>
        /// Initialize AggregateSnapshotStore
        /// </summary>
        /// <typeparam name="TAggregateSnapshotStore"></typeparam>
        /// <param name="enodeConfiguration"></param>
        /// <param name="aggregateSnapshotStoreInitializer"></param>
        /// <returns></returns>
        public static ENodeConfiguration InitializeAggregateSnapshotStore<TAggregateSnapshotStore>(this ENodeConfiguration enodeConfiguration,
            Action<TAggregateSnapshotStore> aggregateSnapshotStoreInitializer)
            where TAggregateSnapshotStore : class, IAggregateSnapshotStore
        {
            aggregateSnapshotStoreInitializer?.Invoke((TAggregateSnapshotStore)ObjectContainer.Resolve<IAggregateSnapshotStore>());
            return enodeConfiguration;
        }
    }
}
