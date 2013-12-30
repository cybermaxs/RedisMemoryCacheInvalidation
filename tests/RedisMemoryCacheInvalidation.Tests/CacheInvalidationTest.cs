using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RedisMemoryCacheInvalidation.Tests
{
    [TestClass]
    public class CacheInvalidationTest
    {
        [TestInitialize]
        public void TestInit()
        {
            CacheInvalidation.RedisBus = null;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            CacheInvalidation.RedisBus = null;
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CacheInvalidation_WhenNotConfigured_ShouldThrowInvalidOperationException()
        {
            CacheInvalidation.CreateChangeMonitor("testkey");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CacheInvalidation_WhenConfiguredTwice_ShouldThrowInvalidOperationException()
        {
            CacheInvalidation.UseRedis(new RedisConnectionInfo());
            CacheInvalidation.UseRedis(new RedisConnectionInfo());
        }

        [TestMethod]
        public void CacheInvalidation_ProgConfig_ShouldSucceed()
        {
            CacheInvalidation.UseRedis(new RedisConnectionInfo());
            
            Assert.IsNotNull(CacheInvalidation.RedisBus);
        }


        [TestMethod]
        public void CacheInvalidation_Config_ShouldSucceed()
        {
            CacheInvalidation.UseRedis();

            Assert.IsNotNull(CacheInvalidation.RedisBus);
        }
    }
}
