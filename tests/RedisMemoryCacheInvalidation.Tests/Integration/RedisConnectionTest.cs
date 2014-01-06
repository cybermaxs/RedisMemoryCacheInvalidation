using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisMemoryCacheInvalidation.Tests.Helper;

namespace RedisMemoryCacheInvalidation.Integration.Tests
{
    [TestClass]
    public class RedisInvalidationMessageBusTest
    {
        [TestMethod]
        public void RedisBus_Ctor_WhenNotStarted_ShouldNotBeConnected()
        {
            RedisConnectionInfo info = new RedisConnectionInfo(host: "pingpong");
            RedisNotificationBus bus = new RedisNotificationBus(info, RedisCacheInvalidationPolicy.ChangeMonitorOnly);

            Thread.Sleep(1000);
            Assert.IsFalse(bus.IsConnected);
        }

        [TestMethod]
        public void RedisBus_Ctor_ShouldBeConnected()
        {
            RedisServer.Start();

            RedisConnectionInfo info = new RedisConnectionInfo();
            RedisNotificationBus bus = new RedisNotificationBus(info, RedisCacheInvalidationPolicy.ChangeMonitorOnly);

            Thread.Sleep(2000);
            Assert.IsTrue(bus.IsConnected);

            RedisServer.Kill();
        }

        [TestMethod]
        public void RedisBus_Failure_ShouldReConnected()
        {
            RedisServer.Start();

            RedisConnectionInfo info = new RedisConnectionInfo();
            RedisNotificationBus bus = new RedisNotificationBus(info, RedisCacheInvalidationPolicy.ChangeMonitorOnly);

            Thread.Sleep(2000);
            Assert.IsTrue(bus.IsConnected);

            RedisServer.Kill();

            Thread.Sleep(2000);
            Assert.IsFalse(bus.IsConnected);

            RedisServer.Start();

            Thread.Sleep(2000);
            Assert.IsTrue(bus.IsConnected);

            bus.Dispose();

            RedisServer.Kill();
        }
    }
}
