using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Monitor;
using RedisMemoryCacheInvalidation.Redis;
using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace RedisMemoryCacheInvalidation.Tests
{
    [TestClass]
    public class RedisNotificationBusTest
    {
        Mock<IRedisConnection> MockOfConnection { get; set; }
        Action<string, string> NotificationEmitter { get; set; }
        private void Invalidate(string topic, Action<string, string> handler)
        {
            this.NotificationEmitter = handler;
        }

        [TestInitialize]
        public void TestInit()
        {
            this.MockOfConnection = new Mock<IRedisConnection>();
            this.MockOfConnection.Setup(c => c.PublishAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(5);
            this.MockOfConnection.Setup(c => c.SubscribeAsync(It.IsAny<string>(), It.IsAny<Action<string, string>>())).Callback<string, Action<string, string>>(this.Invalidate);
            this.MockOfConnection.Setup(c => c.ConnectAsync()).ReturnsAsync(true);
            this.MockOfConnection.Setup(c => c.DisconnectAsync()).Returns((Task)null);
        }

        [TestMethod]
        public void RedisNotificationBus_WhenInvalidCtorArgs_ShouldNotThrowExceptions()
        {
            RedisNotificationBus bus = new RedisNotificationBus("fghfgh");
            Assert.IsNotNull(bus.Connection);
            Assert.IsNotNull(bus.Notifier);
            Assert.AreEqual(MemoryCache.Default, bus.LocalCache);
        }

        [TestMethod]
        public void RedisNotificationBus_WhenStart_ShouldConnectAndSubscribe()
        {
            RedisNotificationBus bus = new RedisNotificationBus("localhost:6379");
            bus.Connection = this.MockOfConnection.Object;

            var startTask = bus.StartAsync();

            Assert.IsNotNull(startTask);
            Assert.IsTrue(startTask.IsCompleted);

            this.MockOfConnection.Verify(c => c.ConnectAsync(), Times.Once);
            this.MockOfConnection.Verify(c => c.SubscribeAsync(RedisNotificationBus.DEFAULT_INVALIDATION_CHANNEL, It.IsAny<Action<string, string>>()), Times.Once);
        }

        [TestMethod]
        public void RedisNotificationBus_WhenNotify_ShouldPublishAsync()
        {
            RedisNotificationBus bus = new RedisNotificationBus("localhost:6379");
            bus.Connection = this.MockOfConnection.Object;


            var notifyTask = bus.NotifyAsync("mykey");

            Assert.IsNotNull(notifyTask);
            Assert.AreEqual(5, notifyTask.Result);
            this.MockOfConnection.Verify(c => c.PublishAsync(RedisNotificationBus.DEFAULT_INVALIDATION_CHANNEL, "mykey"), Times.Once);
        }

        [TestMethod]
        public void RedisNotificationBus_WhenStop_ShouldDisconnect()
        {
            RedisNotificationBus bus = new RedisNotificationBus("localhost:6379");
            bus.Connection = this.MockOfConnection.Object;


            var notifyTask = bus.NotifyAsync("mykey");

            Assert.IsNotNull(notifyTask);
            Assert.AreEqual(5, notifyTask.Result);
            this.MockOfConnection.Verify(c => c.PublishAsync(RedisNotificationBus.DEFAULT_INVALIDATION_CHANNEL, "mykey"), Times.Once);
        }

        [TestMethod]
        public void RedisNotificationBus_WhenDispose_ShouldDisconnect()
        {
            RedisNotificationBus bus = new RedisNotificationBus("localhost:6379");
            bus.Connection = this.MockOfConnection.Object;

            bus.Dispose();

            this.MockOfConnection.Verify(c => c.DisconnectAsync(), Times.Once);
        }

        #region InvalidationMessage
        [TestMethod]
        public void RedisNotificationBus_WhenInvalidation_ShouldRemoveFromDefaultCache()
        {
            MemoryCache lcache = new MemoryCache(Guid.NewGuid().ToString());
            RedisNotificationBus bus = new RedisNotificationBus("localhost:6379", new InvalidationSettings() { TargetCache = lcache, InvalidationStrategy = InvalidationStrategyType.AutoCacheRemoval });
            bus.Connection = this.MockOfConnection.Object;
            RedisChangeMonitor monitor = new RedisChangeMonitor(bus.Notifier, "mykey");
            lcache.Add("mykey", DateTime.UtcNow, new CacheItemPolicy() { AbsoluteExpiration = DateTime.UtcNow.AddDays(1), ChangeMonitors = { monitor } });

            var startTask = bus.StartAsync();

            Assert.IsNotNull(startTask);
            Assert.IsTrue(startTask.IsCompleted);

            //act
            this.NotificationEmitter(RedisNotificationBus.DEFAULT_INVALIDATION_CHANNEL, "mykey");

            //assert
            Assert.IsFalse(lcache.Contains("mykey"));
            Assert.IsTrue(monitor.IsDisposed);
        }

        [TestMethod]
        public void RedisNotificationBus_WhenInvalidation_ShouldDisposeMonitor()
        {
            MemoryCache lcache = new MemoryCache(Guid.NewGuid().ToString());
            RedisNotificationBus bus = new RedisNotificationBus("localhost:6379", new InvalidationSettings() { TargetCache = lcache, InvalidationStrategy = InvalidationStrategyType.ChangeMonitor });
            bus.Connection = this.MockOfConnection.Object;
            RedisChangeMonitor monitor = new RedisChangeMonitor(bus.Notifier, "mykey");
            lcache.Add("mykey", DateTime.UtcNow, new CacheItemPolicy() { AbsoluteExpiration = DateTime.UtcNow.AddDays(1), ChangeMonitors = { monitor } });

            var startTask = bus.StartAsync();

            Assert.IsNotNull(startTask);
            Assert.IsTrue(startTask.IsCompleted);

            //act
            this.NotificationEmitter(RedisNotificationBus.DEFAULT_INVALIDATION_CHANNEL, "mykey");

            //assert
            Assert.IsFalse(lcache.Contains("mykey"));
            Assert.IsTrue(monitor.IsDisposed);
        }

        [TestMethod]
        public void RedisNotificationBus_WhenInvalidation_ShouldInvokeCallback()
        {
            MemoryCache lcache = new MemoryCache(Guid.NewGuid().ToString());
            bool called=false;
            Action<string> cb= s=> {called=true;};
            RedisNotificationBus bus = new RedisNotificationBus("localhost:6379", new InvalidationSettings() { InvalidationStrategy = InvalidationStrategyType.External, InvalidationCallback = cb });
            bus.Connection = this.MockOfConnection.Object;
            RedisChangeMonitor monitor = new RedisChangeMonitor(bus.Notifier, "mykey");
            lcache.Add("mykey", DateTime.UtcNow, new CacheItemPolicy() { AbsoluteExpiration = DateTime.UtcNow.AddDays(1), ChangeMonitors = { monitor } });

            var startTask = bus.StartAsync();

            Assert.IsNotNull(startTask);
            Assert.IsTrue(startTask.IsCompleted);

            //act
            this.NotificationEmitter(RedisNotificationBus.DEFAULT_INVALIDATION_CHANNEL, "mykey");

            //assert
            Assert.IsTrue(lcache.Contains("mykey"));
            Assert.IsFalse(monitor.IsDisposed);
            Assert.IsTrue(called);
        }
        #endregion
    }
}
