﻿using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using BookSleeve;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisMemoryCacheInvalidation.Tests.Helper;

namespace RedisMemoryCacheInvalidation.Integration.Tests
{
    [TestClass]
    public class IntegrationTest
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            RedisServer.Start();
        }

        [ClassCleanup]
        public static void ClassCleanUp()
        {
            RedisServer.Kill();
        }

        [TestMethod]
        public void SingleDep_WhenInvalidation_ShouldRemoved()
        {
            var cache = MemoryCache.Default;
            cache.Trim(100);

            bool removed = false;
            RedisInvalidationMessageBus bus = new RedisInvalidationMessageBus(new RedisConnectionInfo());

            Thread.Sleep(5000);

            CacheItemPolicy policy = new CacheItemPolicy();
            policy.ChangeMonitors.Add(new RedisChangeMonitor(bus, "invalidatenotificationkey"));
            policy.RemovedCallback = args =>
            {
                removed = args.CacheItem.Key == "childkey" && args.RemovedReason == CacheEntryRemovedReason.ChangeMonitorChanged;
            };

            cache.Add("childkey", "childvalue", policy);

            Assert.AreEqual(1, cache.GetCount(), "should have one item");
            Assert.IsFalse(removed, "should not be removed before notification");

            using (var cnx = new RedisConnection("localhost"))
            {
                cnx.Open().Wait();
                cnx.Publish(RedisInvalidationMessageBus.INVALIDATION_KEY, Encoding.Default.GetBytes("invalidatenotificationkey"));
            }

            Thread.Sleep(1000);

            Assert.IsTrue(removed, "shoud be removed");
        }

        [TestMethod]
        public void MultipleDep_WhenInvalidation_ShouldRemoved()
        {
            var cache = MemoryCache.Default;
            cache.Trim(100);
            RedisInvalidationMessageBus bus = new RedisInvalidationMessageBus(new RedisConnectionInfo());

            Thread.Sleep(5000);

            int calls = 0;

            CacheEntryRemovedCallback callback = args =>
                {
                    calls++;
                };

            foreach (var i in Enumerable.Range(1, 10))
            {
                CacheItemPolicy policy = new CacheItemPolicy();
                policy.ChangeMonitors.Add(new RedisChangeMonitor(bus, "invalidatenotificationkey"));
                policy.RemovedCallback = callback;
                cache.Add("childkey" + i.ToString(), "childvalue" + i.ToString(), policy);

            }
            Assert.AreEqual(10, cache.GetCount(), "should have ten items");
            Assert.AreEqual(0, calls, "should never been called");

            using (var cnx = new RedisConnection("localhost"))
            {
                cnx.Open().Wait();
                cnx.Publish(RedisInvalidationMessageBus.INVALIDATION_KEY, Encoding.Default.GetBytes("invalidatenotificationkey"));
            }

            Thread.Sleep(5000);

            Assert.AreEqual(10, calls, "shoud be called ten times");
            Assert.AreEqual(0, cache.GetCount(), "should have ten items");
        }

    }
}
