using ENode.Eventing;
using System.Linq;
using System.Threading.Tasks;

namespace ENode.PublishedVersionStore.Redis
{
    /// <summary>
    /// Represents a redis storage to store the aggregate published event version.
    /// </summary>
    public class RedisPublishedVersionStore : IPublishedVersionStore
    {
        private string _keyPrefix;
        private StackExchange.Redis.IConnectionMultiplexer _connection;
        private StackExchange.Redis.RedisValue _typeField = new StackExchange.Redis.RedisValue("type");
        private StackExchange.Redis.RedisValue _verField = new StackExchange.Redis.RedisValue("version");

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="redisConfiguration"></param>
        /// <param name="keyPrefix"></param>
        public void Initialize(string redisConfiguration, string keyPrefix)
        {
            _keyPrefix = keyPrefix;
            _connection = StackExchange.Redis.ConnectionMultiplexer.Connect(redisConfiguration);
        }

        /// <summary>
        /// Get the current published version for the given aggregate
        /// </summary>
        /// <param name="processorName"></param>
        /// <param name="aggregateRootTypeName"></param>
        /// <param name="aggregateRootId"></param>
        /// <returns></returns>
        public async Task<int> GetPublishedVersionAsync(string processorName, string aggregateRootTypeName, string aggregateRootId)
        {
            var db = _connection.GetDatabase();
            var item = await db.HashGetAllAsync(GetPublishedVersionKey(processorName, aggregateRootId));
            if (item == null || item.Length < 2
                || item.First(f => f.Name == _typeField).Value != aggregateRootTypeName)
            {
                return 0;
            }
            var version = (int)item.First(f => f.Name == _verField).Value;
            return version;
        }

        /// <summary>
        /// Update the published version for the given aggregate.
        /// </summary>
        /// <param name="processorName"></param>
        /// <param name="aggregateRootTypeName"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="publishedVersion"></param>
        /// <returns></returns>
        public async Task UpdatePublishedVersionAsync(string processorName, string aggregateRootTypeName, string aggregateRootId, int publishedVersion)
        {
            const string LUA_SCRIPT = @"
local ver = redis.call('HGET', @key, @versionField);
local type = redis.call('HGET', @key, @typeField);
if not ver then
    redis.call('HSET', @key, @typeField, @typeVal);
    redis.call('HSET', @key, @versionField, @verVal);
    return 1;
elseif ver<@verVal and type==@typeVal then
    redis.call('HSET', @key, @versionField, @verVal);
    return 1;
else
    return 0;
end
";

            var db = _connection.GetDatabase();
            await db.ScriptEvaluateAsync(StackExchange.Redis.LuaScript.Prepare(LUA_SCRIPT), new
            {
                key = GetPublishedVersionKey(processorName, aggregateRootId),
                typeField = _typeField,
                typeVal = aggregateRootTypeName,
                versionField = _verField,
                verVal = publishedVersion
            });
        }

        /// <summary>
        /// Remove key(just for test)
        /// </summary>
        /// <param name="processorName"></param>
        /// <param name="aggregateRootId"></param>
        /// <returns></returns>
        public async Task RemoveKeyAsync(string processorName, string aggregateRootId)
        {
            var db = _connection.GetDatabase();
            await db.KeyDeleteAsync(GetPublishedVersionKey(processorName, aggregateRootId));
        }

        private StackExchange.Redis.RedisKey GetPublishedVersionKey(string processorName, string aggregateRootId)
        {
            return new StackExchange.Redis.RedisKey($"{_keyPrefix}:pv:{processorName}:{aggregateRootId}");
        }
    }
}
