using Moq;
using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Monitor;
using RedisMemoryCacheInvalidation.Redis;
using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Xunit;

namespace RedisMemoryCacheInvalidation.Tests
{
    public class RedisNotificationBusTests
    {
        Mock<IRedisConnection> MockOfConnection { get; set; }
        Action<string, string> NotificationEmitter { get; set; }
        private void Invalidate(string topic, Action<string, string> handler)
        {
            this.NotificationEmitter = handler;
        }

        public RedisNotificationBusTests()
        {
            this.MockOfConnection = new Mock<IRedisConnection>();
            this.MockOfConnection.Setup(c => c.PublishAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(5);
            this.MockOfConnection.Setup(c => c.SubscribeAsync(It.IsAny<string>(), It.IsAny<Action<string, string>>())).Callback<string, Action<string, string>>(this.Invalidate);
            this.MockOfConnection.Setup(c => c.ConnectAsync()).ReturnsAsync(true);
            this.MockOfConnection.Setup(c => c.DisconnectAsync()).Returns((Task)null);
        }

        [Fact]
        public void RedisNotificationBus_WhenInvalidCtorArgs_ShouldNotThrowExceptions()
        {
            var bus = new RedisNotificationBus("fghfgh");
            Assert.NotNull(bus.Connection);
            Assert.NotNull(bus.Notifier);
            Assert.Equal(MemoryCache.Default, bus.LocalCache);
        }

        [Fact]
        public void RedisNotificationBus_WhenStart_ShouldConnectAndSubscribe()
        {
            var bus = new RedisNotificationBus("localhost:6379");
            bus.Connection = this.MockOfConnection.Object;

            var startTask = bus.StartAsync();

            Assert.NotNull(startTask);
            Assert.True(startTask.IsCompleted);

            this.MockOfConnection.Verify(c => c.ConnectAsync(), Times.Once);
            this.MockOfConnection.Verify(c => c.SubscribeAsync(RedisNotificationBus.DEFAULT_INVALIDATION_CHANNEL, It.IsAny<Action<string, string>>()), Times.Once);
        }

        [Fact]
        public void RedisNotificationBus_WhenNotify_ShouldPublishAsync()
        {
            var bus = new RedisNotificationBus("localhost:6379");
            bus.Connection = this.MockOfConnection.Object;


            var notifyTask = bus.NotifyAsync("mykey");

            Assert.NotNull(notifyTask);
            Assert.Equal(5, notifyTask.Result);
            this.MockOfConnection.Verify(c => c.PublishAsync(RedisNotificationBus.DEFAULT_INVALIDATION_CHANNEL, "mykey"), Times.Once);
        }

        [Fact]
        public void RedisNotificationBus_WhenStop_ShouldDisconnect()
        {
            var bus = new RedisNotificationBus("localhost:6379");
            bus.Connection = this.MockOfConnection.Object;


            var notifyTask = bus.NotifyAsync("mykey");

            Assert.NotNull(notifyTask);
            Assert.Equal(5, notifyTask.Result);
            this.MockOfConnection.Verify(c => c.PublishAsync(RedisNotificationBus.DEFAULT_INVALIDATION_CHANNEL, "mykey"), Times.Once);
        }

        [Fact]
        public void RedisNotificationBus_WhenDispose_ShouldDisconnect()
        {
            var bus = new RedisNotificationBus("localhost:6379");
            bus.Connection = this.MockOfConnection.Object;

            bus.Dispose();

            this.MockOfConnection.Verify(c => c.DisconnectAsync(), Times.Once);
        }

        #region InvalidationMessage
        [Fact]
        public void RedisNotificationBus_WhenInvalidation_ShouldRemoveFromDefaultCache()
        {
            var lcache = new MemoryCache(Guid.NewGuid().ToString());
            var bus = new RedisNotificationBus("localhost:6379", new InvalidationSettings() { TargetCache = lcache, InvalidationStrategy = InvalidationStrategyType.AutoCacheRemoval });
            bus.Connection = this.MockOfConnection.Object;
            var monitor = new RedisChangeMonitor(bus.Notifier, "mykey");
            lcache.Add("mykey", DateTime.UtcNow, new CacheItemPolicy() { AbsoluteExpiration = DateTime.UtcNow.AddDays(1), ChangeMonitors = { monitor } });

            var startTask = bus.StartAsync();

            Assert.NotNull(startTask);
            Assert.True(startTask.IsCompleted);

            //act
            this.NotificationEmitter(RedisNotificationBus.DEFAULT_INVALIDATION_CHANNEL, "mykey");

            //assert
            Assert.False(lcache.Contains("mykey"));
            Assert.True(monitor.IsDisposed);
        }

        [Fact]
        public void RedisNotificationBus_WhenInvalidation_ShouldDisposeMonitor()
        {
            var lcache = new MemoryCache(Guid.NewGuid().ToString());
            var bus = new RedisNotificationBus("localhost:6379", new InvalidationSettings() { TargetCache = lcache, InvalidationStrategy = InvalidationStrategyType.ChangeMonitor });
            bus.Connection = this.MockOfConnection.Object;
            var monitor = new RedisChangeMonitor(bus.Notifier, "mykey");
            lcache.Add("mykey", DateTime.UtcNow, new CacheItemPolicy() { AbsoluteExpiration = DateTime.UtcNow.AddDays(1), ChangeMonitors = { monitor } });

            var startTask = bus.StartAsync();

            Assert.NotNull(startTask);
            Assert.True(startTask.IsCompleted);

            //act
            this.NotificationEmitter(RedisNotificationBus.DEFAULT_INVALIDATION_CHANNEL, "mykey");

            //assert
            Assert.False(lcache.Contains("mykey"));
            Assert.True(monitor.IsDisposed);
        }

        [Fact]
        public void RedisNotificationBus_WhenInvalidation_ShouldInvokeCallback()
        {
            var lcache = new MemoryCache(Guid.NewGuid().ToString());
            var called=false;
            Action<string> cb= s=> {called=true;};
            var bus = new RedisNotificationBus("localhost:6379", new InvalidationSettings() { InvalidationStrategy = InvalidationStrategyType.External, InvalidationCallback = cb });
            bus.Connection = this.MockOfConnection.Object;
            var monitor = new RedisChangeMonitor(bus.Notifier, "mykey");
            lcache.Add("mykey", DateTime.UtcNow, new CacheItemPolicy() { AbsoluteExpiration = DateTime.UtcNow.AddDays(1), ChangeMonitors = { monitor } });

            var startTask = bus.StartAsync();

            Assert.NotNull(startTask);
            Assert.True(startTask.IsCompleted);

            //act
            this.NotificationEmitter(RedisNotificationBus.DEFAULT_INVALIDATION_CHANNEL, "mykey");

            //assert
            Assert.True(lcache.Contains("mykey"));
            Assert.False(monitor.IsDisposed);
            Assert.True(called);
        }
        #endregion
    }
}
