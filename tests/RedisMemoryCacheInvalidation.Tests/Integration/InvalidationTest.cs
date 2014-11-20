using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Monitor;
using RedisMemoryCacheInvalidation.Tests.Helper;
using StackExchange.Redis;
using System;
using System.Runtime.Caching;
using System.Text;
using System.Threading;

namespace RedisMemoryCacheInvalidation.Integration.Tests
{
    [TestClass]
    public class InvalidationTest
    {
        private static MemoryCache LocalCache { get; set; }

        #region ClassInit & CleanUp
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            RedisServer.Start();
            LocalCache = new MemoryCache(Guid.NewGuid().ToString());
            InvalidationManager.ConfigureAsync("localhost:6379", new InvalidationSettings() { InvalidationStrategy = InvalidationStrategyType.All, EnableKeySpaceNotifications = true, TargetCache=LocalCache }).Wait();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            RedisServer.Kill();
        }
        #endregion

        [TestInitialize]
        public void TestInit()
        {
            //reset cache
            LocalCache.Trim(100);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void MultiplesDeps_WhenChangeMonitor_WhenInvalidation_ShouldRemoved()
        {
            string baseCacheKey = "mykey";
            string invalidationKey = "bluescreen";
            var monitor1 = InvalidationManager.CreateChangeMonitor(invalidationKey);
            var monitor2 = InvalidationManager.CreateChangeMonitor(invalidationKey);

            this.CreateCacheItemAndAdd(LocalCache, baseCacheKey + "1", monitor1);
            this.CreateCacheItemAndAdd(LocalCache, baseCacheKey + "2", monitor2);

            Assert.AreEqual(2, LocalCache.GetCount(), "should have two items");
            Assert.IsFalse(monitor1.IsDisposed, "should not be removed before notification");
            Assert.IsFalse(monitor2.IsDisposed, "should not be removed before notification");

            //act 
            using (var cnx = ConnectionMultiplexer.Connect("localhost:6379"))
            {
                cnx.GetSubscriber().Publish(RedisNotificationBus.DEFAULT_INVALIDATION_CHANNEL, Encoding.Default.GetBytes(invalidationKey));
            }

            // hack wait for notif
            Thread.Sleep(50);

            //assert
            Assert.IsFalse(LocalCache.Contains(baseCacheKey + "1"), "cache item shoud be removed");
            Assert.IsFalse(LocalCache.Contains(baseCacheKey + "2"), "cache item shoud be removed");
            Assert.IsTrue(monitor1.IsDisposed, "should be disposed");
            Assert.IsTrue(monitor2.IsDisposed, "should be disposed");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void MultiplesDeps_WhenImplicitRemoval_WhenInvalidation_ShouldRemoved()
        {
            string baseCacheKey = "mykey";

            this.CreateCacheItemAndAdd(LocalCache, baseCacheKey + "1");
            this.CreateCacheItemAndAdd(LocalCache, baseCacheKey + "2");

            Assert.AreEqual(2, LocalCache.GetCount(), "should have two items");

            // act 
            InvalidationManager.InvalidateAsync(baseCacheKey + "1").Wait();
            InvalidationManager.InvalidateAsync(baseCacheKey + "2").Wait();

            Thread.Sleep(50);

            //assert
            Assert.AreEqual(0, LocalCache.GetCount(), "should have 0 items");
            Assert.IsFalse(LocalCache.Contains(baseCacheKey + "1"), "cache item shoud be removed");
            Assert.IsFalse(LocalCache.Contains(baseCacheKey + "2"), "cache item shoud be removed");
        }


        [TestMethod]
        [TestCategory("Integration")]
        public void MultiplesDeps_WhenSpaceNotification_ShouldBeRemoved()
        {
            string baseCacheKey = "mykey";
            string invalidationKey = "bluescreen";
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
            Assert.AreEqual(0, LocalCache.GetCount(), "should have 0 items");
            Assert.IsFalse(LocalCache.Contains(baseCacheKey + "1"), "cache item shoud be removed");
            Assert.IsFalse(LocalCache.Contains(baseCacheKey + "2"), "cache item shoud be removed");
        }

        private CacheItem CreateCacheItemAndAdd(MemoryCache target, string cacheKey, RedisChangeMonitor monitor = null)
        {
            CacheItem cacheItem = new CacheItem(cacheKey, DateTime.Now);
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.UtcNow.AddDays(1);
            if(monitor!=null)
                policy.ChangeMonitors.Add(monitor);
            target.Add(cacheItem, policy);
            return cacheItem;
        }
    }
}
