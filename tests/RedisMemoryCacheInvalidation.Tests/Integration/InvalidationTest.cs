using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Monitor;
using RedisMemoryCacheInvalidation.Tests.Fixtures;
using StackExchange.Redis;
using System;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using Xunit;

namespace RedisMemoryCacheInvalidation.Integration.Tests
{
    [Collection("RedisServer")]
    public class InvalidationTests
    {
        private static MemoryCache LocalCache { get; set; }


        public InvalidationTests(RedisServerFixture redisServer)
        {
            LocalCache = new MemoryCache(Guid.NewGuid().ToString());
            redisServer.Reset();
            InvalidationManager.ConfigureAsync("localhost:6379", new InvalidationSettings() { InvalidationStrategy = InvalidationStrategyType.All, EnableKeySpaceNotifications = true, TargetCache = LocalCache }).Wait();

            //reset cache
            LocalCache.Trim(100);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void MultiplesDeps_WhenChangeMonitor_WhenInvalidation_ShouldRemoved()
        {
            var baseCacheKey = "mykey";
            var invalidationKey = "bluescreen";
            var monitor1 = InvalidationManager.CreateChangeMonitor(invalidationKey);
            var monitor2 = InvalidationManager.CreateChangeMonitor(invalidationKey);

            this.CreateCacheItemAndAdd(LocalCache, baseCacheKey + "1", monitor1);
            this.CreateCacheItemAndAdd(LocalCache, baseCacheKey + "2", monitor2);

            Assert.Equal(2, LocalCache.GetCount());
            Assert.False(monitor1.IsDisposed, "should not be removed before notification");
            Assert.False(monitor2.IsDisposed, "should not be removed before notification");

            //act 
            using (var cnx = ConnectionMultiplexer.Connect("localhost:6379"))
            {
                cnx.GetSubscriber().Publish(RedisNotificationBus.DEFAULT_INVALIDATION_CHANNEL, Encoding.Default.GetBytes(invalidationKey));
            }

            // hack wait for notif
            Thread.Sleep(50);

            //assert
            Assert.False(LocalCache.Contains(baseCacheKey + "1"), "cache item shoud be removed");
            Assert.False(LocalCache.Contains(baseCacheKey + "2"), "cache item shoud be removed");
            Assert.True(monitor1.IsDisposed, "should be disposed");
            Assert.True(monitor2.IsDisposed, "should be disposed");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void MultiplesDeps_WhenImplicitRemoval_WhenInvalidation_ShouldRemoved()
        {
            var baseCacheKey = "mykey";

            this.CreateCacheItemAndAdd(LocalCache, baseCacheKey + "1");
            this.CreateCacheItemAndAdd(LocalCache, baseCacheKey + "2");

            Assert.Equal(2, LocalCache.GetCount());

            // act 
            InvalidationManager.InvalidateAsync(baseCacheKey + "1").Wait();
            InvalidationManager.InvalidateAsync(baseCacheKey + "2").Wait();

            Thread.Sleep(50);

            //assert
            Assert.Equal(0, LocalCache.GetCount());
            Assert.False(LocalCache.Contains(baseCacheKey + "1"), "cache item shoud be removed");
            Assert.False(LocalCache.Contains(baseCacheKey + "2"), "cache item shoud be removed");
        }


        [Fact]
        [Trait("Category", "Integration")]
        public void MultiplesDeps_WhenSpaceNotification_ShouldBeRemoved()
        {
            var baseCacheKey = "mykey";
            var invalidationKey = "bluescreen";
            var monitor1 = InvalidationManager.CreateChangeMonitor(invalidationKey);
            var monitor2 = InvalidationManager.CreateChangeMonitor(invalidationKey);

            this.CreateCacheItemAndAdd(LocalCache, baseCacheKey + "1", monitor1);
            this.CreateCacheItemAndAdd(LocalCache, baseCacheKey + "2", monitor2);


            // act 
            using (var cnx = ConnectionMultiplexer.Connect("localhost:6379"))
            {
                cnx.GetDatabase().StringSet(invalidationKey, "notused");
            }

            Thread.Sleep(50);

            //assert
            Assert.Equal(0, LocalCache.GetCount());
            Assert.False(LocalCache.Contains(baseCacheKey + "1"), "cache item shoud be removed");
            Assert.False(LocalCache.Contains(baseCacheKey + "2"), "cache item shoud be removed");
        }

        private CacheItem CreateCacheItemAndAdd(MemoryCache target, string cacheKey, RedisChangeMonitor monitor = null)
        {
            var cacheItem = new CacheItem(cacheKey, DateTime.Now);
            var policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.UtcNow.AddDays(1);
            if(monitor!=null)
                policy.ChangeMonitors.Add(monitor);
            target.Add(cacheItem, policy);
            return cacheItem;
        }
    }
}
