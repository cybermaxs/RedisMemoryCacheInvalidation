using System;
using System.Runtime.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RedisMemoryCacheInvalidation.Tests
{
    [TestClass]
    public class CacheInvalidationTest
    {
        [TestInitialize]
        public void TestInit()
        {
            RedisCacheInvalidation.RedisBus = null;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            RedisCacheInvalidation.RedisBus = null;
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateChangeMonitor_WhenNotConfigured_ShouldThrowInvalidOperationException()
        {
            RedisCacheInvalidation.CreateChangeMonitor("testkey");
        }

        [TestMethod]
        public void CreateChangeMonitor_WhenConfigured_ShouldSucceed()
        {
            CacheItem item = new CacheItem("mykey");
            RedisCacheInvalidation.Use(new RedisConnectionInfo());
            var monitor = RedisCacheInvalidation.CreateChangeMonitor(item);

            Assert.IsNotNull(monitor);

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CacheInvalidation_WhenConfiguredTwice_ShouldThrowInvalidOperationException()
        {
            RedisCacheInvalidation.Use(new RedisConnectionInfo());
            RedisCacheInvalidation.Use(new RedisConnectionInfo());
        }




        [TestMethod]
        public void CacheInvalidation_ProgConfig_ShouldSucceed()
        {
            RedisCacheInvalidation.Use(new RedisConnectionInfo());

            Assert.IsNotNull(RedisCacheInvalidation.RedisBus);
        }


        [TestMethod]
        public void CacheInvalidation_Config_ShouldSucceed()
        {
            RedisCacheInvalidation.Use();

            Assert.IsNotNull(RedisCacheInvalidation.RedisBus);
        }
    }
}
