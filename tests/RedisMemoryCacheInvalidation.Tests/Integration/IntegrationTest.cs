using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Tests.Helper;
using StackExchange.Redis;
using System;
using System.Runtime.Caching;
using System.Text;
using System.Threading;

namespace RedisMemoryCacheInvalidation.Integration.Tests
{
    [TestClass]
    public class IntegrationTest
    {
        private static MemoryCache LocalCache { get; set; }

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            RedisServer.Start();
            LocalCache = new MemoryCache(Guid.NewGuid().ToString());
            InvalidationManager.ConfigureAsync("localhost:6379", new InvalidationSettings() { InvalidationStrategy = InvalidationStrategyType.All, EnableKeySpaceNotifications = true }).Wait();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            RedisServer.Kill();
        }

        [TestInitialize]
        public void TestInit()
        {
            LocalCache.Trim(100);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void SingleDepWithChangeMonitor_WhenInvalidation_ShouldRemoved()
        {
            string cachekey = "mykey";
            string invalidkey = "bluescreen";
            var monitor = InvalidationManager.CreateChangeMonitor(invalidkey);

            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.UtcNow.AddDays(1);
            policy.ChangeMonitors.Add(monitor);
            LocalCache.Add(cachekey, Guid.NewGuid(), policy);

            Assert.AreEqual(1, LocalCache.GetCount(), "should have one item");
            Assert.IsFalse(monitor.IsDisposed, "should not be removed before notification");

            //act 
            using (var cnx = ConnectionMultiplexer.Connect("localhost:6379"))
            {
                cnx.GetSubscriber().Publish(RedisNotificationBus.DEFAULT_INVALIDATION_CHANNEL, Encoding.Default.GetBytes(invalidkey));
            }

            // hack wait for notif
            Thread.Sleep(50);

            //assert
            Assert.IsFalse(LocalCache.Contains(cachekey), "cache item shoud be removed");
            Assert.IsTrue(monitor.IsDisposed, "should be disposed");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void DoubleDepsWithChangeMonitor_WhenInvalidation_ShouldRemoved()
        {
            string cachekey = "mykey";
            string invalidkey = "bluescreen";
            var monitor1 = InvalidationManager.CreateChangeMonitor(invalidkey);
            var monitor2 = InvalidationManager.CreateChangeMonitor(invalidkey);

            CacheItemPolicy policy1 = new CacheItemPolicy();
            policy1.AbsoluteExpiration = DateTime.UtcNow.AddDays(1);
            policy1.ChangeMonitors.Add(monitor1);
            LocalCache.Add(cachekey + "1", Guid.NewGuid(), policy1);

            CacheItemPolicy policy2 = new CacheItemPolicy();
            policy2.AbsoluteExpiration = DateTime.UtcNow.AddDays(1);
            policy2.ChangeMonitors.Add(monitor2);
            LocalCache.Add(cachekey + "2", Guid.NewGuid(), policy2);

            Assert.AreEqual(2, LocalCache.GetCount(), "should have two items");
            Assert.IsFalse(monitor1.IsDisposed, "should not be removed before notification");
            Assert.IsFalse(monitor2.IsDisposed, "should not be removed before notification");

            //act 
            using (var cnx = ConnectionMultiplexer.Connect("localhost:6379"))
            {
                cnx.GetSubscriber().Publish(RedisNotificationBus.DEFAULT_INVALIDATION_CHANNEL, Encoding.Default.GetBytes(invalidkey));
            }

            // hack wait for notif
            Thread.Sleep(50);

            //assert
            Assert.IsFalse(LocalCache.Contains(cachekey + "1"), "cache item shoud be removed");
            Assert.IsFalse(LocalCache.Contains(cachekey + "2"), "cache item shoud be removed");
            Assert.IsTrue(monitor1.IsDisposed, "should be disposed");
            Assert.IsTrue(monitor2.IsDisposed, "should be disposed");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void SingleDepWithImplicitRemoval_WhenInvalidation_ShouldRemoved()
        {
            string cachekey = "mykey";
            LocalCache.Add(cachekey, Guid.NewGuid(), DateTime.UtcNow.AddDays(1));

            // act 
            InvalidationManager.InvalidateAsync(cachekey);

            Thread.Sleep(50);

            //assert
            Assert.IsFalse(LocalCache.Contains(cachekey), "cache item shoud be removed");
        }


        [TestMethod]
        [TestCategory("Integration")]
        public void SingleKeySpaceNotification_ShouldBeRemoved()
        {
            string cachekey = "mykey";
            string invalidkey = "bluescreen";
            var monitor = InvalidationManager.CreateChangeMonitor(invalidkey);

            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.UtcNow.AddDays(1);
            policy.ChangeMonitors.Add(monitor);
            LocalCache.Add(cachekey, Guid.NewGuid(), policy);

            LocalCache.Add(cachekey, Guid.NewGuid(), policy);

            // act 
            using (var cnx = ConnectionMultiplexer.Connect("localhost:6379"))
            {
                cnx.GetDatabase().StringSet(invalidkey, "truc");
            }

            Thread.Sleep(150);

            //assert
            Assert.IsFalse(LocalCache.Contains(cachekey), "cache item shoud be removed");
        }
    }
}
