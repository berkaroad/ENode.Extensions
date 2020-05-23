using System;
using Xunit;

namespace ENode.PublishedVersionStore.Redis.Tests
{
    public class RedisPublishedVersionStoreTests
    {
        private string _redisConfiguration = "127.0.0.1:6379,syncTimeout=3000,defaultDatabase=0,name=inventory_control,allowAdmin=false";

        [Fact]
        public void GetPublishedVersion()
        {
            var store = new RedisPublishedVersionStore();
            store.Initialize(_redisConfiguration, "ic");
            var aggregateRootId = $"B{DateTime.Now.Ticks}";

            var ver = store.GetPublishedVersionAsync("p1", "StockBox", aggregateRootId).Result;
            Assert.Equal(0, ver);

            store.UpdatePublishedVersionAsync("p1", "StockBox", aggregateRootId, 1).Wait();
            ver = store.GetPublishedVersionAsync("p1", "StockBox", aggregateRootId).Result;
            Assert.Equal(1, ver);

            store.RemoveKeyAsync("p1", aggregateRootId).Wait();
        }

        [Fact]
        public void UpdatePublishedVersion()
        {
            var store = new RedisPublishedVersionStore();
            store.Initialize(_redisConfiguration, "ic");
            var aggregateRootId = $"B{DateTime.Now.Ticks}";

            store.UpdatePublishedVersionAsync("p1", "StockBox", aggregateRootId, 1).Wait();
            var ver1 = store.GetPublishedVersionAsync("p1", "StockBox", aggregateRootId).Result;
            Assert.Equal(1, ver1);

            store.UpdatePublishedVersionAsync("p1", "StockBox", aggregateRootId, 5).Wait();
            var ver2 = store.GetPublishedVersionAsync("p1", "StockBox", aggregateRootId).Result;
            Assert.Equal(5, ver2);

            store.UpdatePublishedVersionAsync("p1", "StockBox", aggregateRootId, 3).Wait();
            var ver3 = store.GetPublishedVersionAsync("p1", "StockBox", aggregateRootId).Result;
            Assert.Equal(5, ver3);
            store.RemoveKeyAsync("p1", aggregateRootId).Wait();
        }
    }
}
