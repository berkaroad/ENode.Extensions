using ECommon.Components;
using ENode.Configurations;
using ENode.Eventing;

namespace ENode.PublishedVersionStore.Redis
{
    /// <summary>
    /// ENode extensions
    /// </summary>
    public static class ENodeExtensions
    {
        /// <summary>
        /// Use redis for published version store
        /// </summary>
        /// <param name="eNodeConfiguration"></param>
        /// <returns></returns>
        public static ENodeConfiguration UseRedisPublishedVersionStore(this ENodeConfiguration eNodeConfiguration)
        {
            eNodeConfiguration.GetCommonConfiguration().SetDefault<IPublishedVersionStore, RedisPublishedVersionStore>((string)null, LifeStyle.Singleton);
            return eNodeConfiguration;
        }

        /// <summary>
        /// Initialize redis for published version store
        /// </summary>
        /// <param name="eNodeConfiguration"></param>
        /// <param name="redisConfiguration"></param>
        /// <param name="keyPrefix"></param>
        /// <returns></returns>
        public static ENodeConfiguration InitializeRedisPublishedVersionStore(this ENodeConfiguration eNodeConfiguration, string redisConfiguration, string keyPrefix)
        {
            ((RedisPublishedVersionStore)ObjectContainer.Resolve<IPublishedVersionStore>()).Initialize(redisConfiguration, keyPrefix);
            return eNodeConfiguration;
        }
    }
}
