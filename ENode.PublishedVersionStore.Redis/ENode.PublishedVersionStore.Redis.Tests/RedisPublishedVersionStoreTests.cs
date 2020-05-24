using System;
using Xunit;

namespace ENode.PublishedVersionStore.Redis.Tests
{
    public class RedisPublishedVersionStoreTests
    {
        private string _redisConfiguration = "127.0.0.1:6379,syncTimeout=3000,defaultDatabase=0,name=demo,allowAdmin=false";

        [Fact]
        public void GetPublishedVersion()
        {
            var store = new RedisPublishedVersionStore();
            store.Initialize(_redisConfiguration, "demo");
            var aggregateRootId = $"B{DateTime.Now.Ticks}";
            try
            {
                var ver1 = store.GetPublishedVersionAsync("p1", "StockBox", aggregateRootId).Result;
                Assert.Equal(0, ver1);

                store.UpdatePublishedVersionAsync("p1", "StockBox", aggregateRootId, 1).Wait();
                var ver2 = store.GetPublishedVersionAsync("p1", "StockBox", aggregateRootId).Result;
                Assert.Equal(1, ver2);
            }
            finally
            {
                store.RemoveKeyAsync("p1", aggregateRootId).Wait();
            }
        }

        [Fact]
        public void GetPublishedVersionWithDifferentAggregateRootType()
        {
            var store = new RedisPublishedVersionStore();
            store.Initialize(_redisConfiguration, "demo");
            var aggregateRootId = $"B{DateTime.Now.Ticks}";
            try
            {
                store.UpdatePublishedVersionAsync("p1", "StockBox", aggregateRootId, 1).Wait();
                var ver1 = store.GetPublishedVersionAsync("p1", "StockBox", aggregateRootId).Result;
                Assert.Equal(1, ver1);

                var ver2 = store.GetPublishedVersionAsync("p1", "DownGoodsBill", aggregateRootId).Result;
                Assert.Equal(0, ver2);
            }
            finally
            {
                store.RemoveKeyAsync("p1", aggregateRootId).Wait();
            }
        }

        [Fact]
        public void UpdatePublishedVersionWithLessVersion()
        {
            var store = new RedisPublishedVersionStore();
            store.Initialize(_redisConfiguration, "demo");
            var aggregateRootId = $"B{DateTime.Now.Ticks}";
            try
            {
                store.UpdatePublishedVersionAsync("p1", "StockBox", aggregateRootId, 1).Wait();
                var ver1 = store.GetPublishedVersionAsync("p1", "StockBox", aggregateRootId).Result;
                Assert.Equal(1, ver1);

                store.UpdatePublishedVersionAsync("p1", "StockBox", aggregateRootId, 5).Wait();
                var ver2 = store.GetPublishedVersionAsync("p1", "StockBox", aggregateRootId).Result;
                Assert.Equal(5, ver2);

                store.UpdatePublishedVersionAsync("p1", "StockBox", aggregateRootId, 3).Wait();
                var ver3 = store.GetPublishedVersionAsync("p1", "StockBox", aggregateRootId).Result;
                Assert.Equal(5, ver3);
            }
            finally
            {
                store.RemoveKeyAsync("p1", aggregateRootId).Wait();
            }
        }

        [Fact]
        public void UpdatePublishedVersionWithDifferentAggregateRootType()
        {
            var store = new RedisPublishedVersionStore();
            store.Initialize(_redisConfiguration, "demo");
            var aggregateRootId = $"B{DateTime.Now.Ticks}";
            try
            {
                store.UpdatePublishedVersionAsync("p1", "StockBox", aggregateRootId, 1).Wait();
                var ver1 = store.GetPublishedVersionAsync("p1", "StockBox", aggregateRootId).Result;
                Assert.Equal(1, ver1);

                store.UpdatePublishedVersionAsync("p1", "DownGoodsBill", aggregateRootId, 2).Wait();
                var ver2 = store.GetPublishedVersionAsync("p1", "StockBox", aggregateRootId).Result;
                Assert.Equal(1, ver2);
            }
            finally
            {
                store.RemoveKeyAsync("p1", aggregateRootId).Wait();
            }
        }
    }
}
