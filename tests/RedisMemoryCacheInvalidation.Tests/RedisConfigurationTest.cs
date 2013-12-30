using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisMemoryCacheInvalidation.Configuration;

namespace RedisMemoryCacheInvalidation.Tests
{
    [TestClass]
    public class RedisConfigurationTest
    {
        [TestMethod]
        public void ValidConfig_WhenLoad_ShouldSuccess()
        {
            ConfigurationProvider<RedisConfigurationSection> provider = new ConfigurationProvider<RedisConfigurationSection>(RedisConfigurationSection.SECTION_NAME);
            provider.SetConfigurationFile(@"Config/Valid.config");

            var redisconfig = provider.Read();

            Assert.IsNotNull(redisconfig);
            Assert.AreEqual("localhostv2", redisconfig.Host);
            Assert.AreEqual(true, redisconfig.AllowAdmin);
            Assert.AreEqual(18, redisconfig.IOTimeout);
            Assert.AreEqual(52, redisconfig.SyncTimeout);
            Assert.AreEqual(34, redisconfig.MaxUnsent);
            Assert.AreEqual(666, redisconfig.Port);
            Assert.AreEqual("pong", redisconfig.Password);

        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void CurrentConfig_WhenInvalidSectionName_ShouldThrowConfigurationException()
        {
            ConfigurationProvider<RedisConfigurationSection> provider = new ConfigurationProvider<RedisConfigurationSection>("truc");

            var redisconfig = provider.Read();
        }

        [TestMethod]
        public void CurrentConfig_WhenLoaded_WhenLoad_ShouldSuccess()
        {
            ConfigurationProvider<RedisConfigurationSection> provider = new ConfigurationProvider<RedisConfigurationSection>(RedisConfigurationSection.SECTION_NAME);

            var redisconfig = provider.Read();

            Assert.IsNotNull(redisconfig);
            Assert.AreEqual("localhost", redisconfig.Host);
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void InvalidConfig_WhenLoaded_ShouldThrowConfigurationException()
        {
            ConfigurationProvider<RedisConfigurationSection> provider = new ConfigurationProvider<RedisConfigurationSection>(RedisConfigurationSection.SECTION_NAME);
            provider.SetConfigurationFile(@"Config/Invalid.config");

            var redisconfig = provider.Read();
        }

    }
}
