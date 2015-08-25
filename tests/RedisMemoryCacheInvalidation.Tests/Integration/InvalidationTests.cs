using Ploeh.AutoFixture;
using RedisMemoryCacheInvalidation.Monitor;
using RedisMemoryCacheInvalidation.Tests;
using RedisMemoryCacheInvalidation.Tests.Fixtures;
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
        private readonly MemoryCache localCache;
        private readonly Fixture fixture;
        private RedisServerFixture redis;

        public InvalidationTests(RedisServerFixture redisServer)
        {
            localCache = new MemoryCache(Guid.NewGuid().ToString());
            localCache.Trim(100);

            redisServer.Reset();
            redis = redisServer;

            InvalidationManager.notificationBus = null;
            InvalidationManager.Configure("localhost:6379", new InvalidationSettings {
                InvalidationStrategy = InvalidationStrategyType.All,
                EnableKeySpaceNotifications = true,
                TargetCache = localCache });

            fixture = new Fixture();
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.IntegrationTestCategory)]
        public void MultiplesDeps_WhenChangeMonitor_WhenInvalidation_ShouldRemoved()
        {
            var baseCacheKey = fixture.Create<string>();
            var invalidationKey = fixture.Create<string>();

            var monitor1 = InvalidationManager.CreateChangeMonitor(invalidationKey);
            var monitor2 = InvalidationManager.CreateChangeMonitor(invalidationKey);

            CreateCacheItemAndAdd(localCache, baseCacheKey + "1", monitor1);
            CreateCacheItemAndAdd(localCache, baseCacheKey + "2", monitor2);

            Assert.Equal(2, localCache.GetCount());
            Assert.False(monitor1.IsDisposed, "should not be removed before notification");
            Assert.False(monitor2.IsDisposed, "should not be removed before notification");

            //act 
            var subscriber = redis.GetSubscriber();
            subscriber.Publish(Constants.DEFAULT_INVALIDATION_CHANNEL, Encoding.Default.GetBytes(invalidationKey));

            // hack wait for notif
            Thread.Sleep(50);

            //assert
            Assert.False(localCache.Contains(baseCacheKey + "1"), "cache item shoud be removed");
            Assert.False(localCache.Contains(baseCacheKey + "2"), "cache item shoud be removed");
            Assert.True(monitor1.IsDisposed, "should be disposed");
            Assert.True(monitor2.IsDisposed, "should be disposed");
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.IntegrationTestCategory)]
        public void MultiplesDeps_WhenImplicitRemoval_WhenInvalidation_ShouldRemoved()
        {
            var baseCacheKey = fixture.Create<string>();

            CreateCacheItemAndAdd(localCache, baseCacheKey + "1");
            CreateCacheItemAndAdd(localCache, baseCacheKey + "2");

            Assert.Equal(2, localCache.GetCount());

            // act 
            InvalidationManager.InvalidateAsync(baseCacheKey + "1").Wait();
            InvalidationManager.InvalidateAsync(baseCacheKey + "2").Wait();

            Thread.Sleep(50);

            //assert
            Assert.Equal(0, localCache.GetCount());
            Assert.False(localCache.Contains(baseCacheKey + "1"), "cache item shoud be removed");
            Assert.False(localCache.Contains(baseCacheKey + "2"), "cache item shoud be removed");
        }


        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.IntegrationTestCategory)]
        public void MultiplesDeps_WhenSpaceNotification_ShouldBeRemoved()
        {
            var baseCacheKey = fixture.Create<string>();
            var invalidationKey = fixture.Create<string>();

            var monitor1 = InvalidationManager.CreateChangeMonitor(invalidationKey);
            var monitor2 = InvalidationManager.CreateChangeMonitor(invalidationKey);

            CreateCacheItemAndAdd(localCache, baseCacheKey + "1", monitor1);
            CreateCacheItemAndAdd(localCache, baseCacheKey + "2", monitor2);

            // act 
            var db = redis.GetDatabase(0);
            db.StringSet(invalidationKey, "notused");

            Thread.Sleep(200);

            //assert
            Assert.Equal(0, localCache.GetCount());
            Assert.False(localCache.Contains(baseCacheKey + "1"), "cache item shoud be removed");
            Assert.False(localCache.Contains(baseCacheKey + "2"), "cache item shoud be removed");
        }

        private static CacheItem CreateCacheItemAndAdd(MemoryCache target, string cacheKey, RedisChangeMonitor monitor = null)
        {
            var cacheItem = new CacheItem(cacheKey, DateTime.Now);
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.UtcNow.AddDays(1)
            };
            if (monitor != null)
                policy.ChangeMonitors.Add(monitor);
            target.Add(cacheItem, policy);
            return cacheItem;
        }
    }
}
