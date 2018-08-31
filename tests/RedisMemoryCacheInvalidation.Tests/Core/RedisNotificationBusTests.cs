using System;
using System.Runtime.Caching;
using Moq;
using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Monitor;
using RedisMemoryCacheInvalidation.Redis;
using StackExchange.Redis;
using Xunit;

namespace RedisMemoryCacheInvalidation.Tests.Core
{
    public class RedisNotificationBusTests
    {
        public RedisNotificationBusTests()
        {
            MockOfConnection = new Mock<IRedisConnection>();
            MockOfConnection.Setup(c => c.PublishAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(5);
            MockOfConnection.Setup(c => c.Subscribe(It.IsAny<string>(), It.IsAny<Action<RedisChannel, RedisValue>>()))
                .Callback<string, Action<RedisChannel, RedisValue>>(Invalidate);
            MockOfConnection.Setup(c => c.Connect()).Returns(true);
            MockOfConnection.Setup(c => c.Disconnect());
        }

        private Mock<IRedisConnection> MockOfConnection { get; }
        private Action<RedisChannel, RedisValue> NotificationEmitter { get; set; }

        private void Invalidate(string topic, Action<RedisChannel, RedisValue> handler)
        {
            NotificationEmitter = handler;
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void RedisNotificationBus_WhenInvalidation_ShouldDisposeMonitor()
        {
            var lcache = new MemoryCache(Guid.NewGuid().ToString());
            var bus = new RedisNotificationBus("localhost:6379",
                new InvalidationSettings
                    {TargetCache = lcache, InvalidationStrategy = InvalidationStrategyType.ChangeMonitor});
            bus.Connection = MockOfConnection.Object;
            var monitor = new RedisChangeMonitor(bus.Notifier, "mykey");
            lcache.Add("mykey", DateTime.UtcNow,
                new CacheItemPolicy {AbsoluteExpiration = DateTime.UtcNow.AddDays(1), ChangeMonitors = {monitor}});

            bus.Start();

            //act
            NotificationEmitter(Constants.DEFAULT_INVALIDATION_CHANNEL, "mykey");

            //assert
            Assert.False(lcache.Contains("mykey"));
            Assert.True(monitor.IsDisposed);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void RedisNotificationBus_WhenInvalidation_ShouldInvokeCallback()
        {
            var lcache = new MemoryCache(Guid.NewGuid().ToString());
            var called = false;
            Action<string> cb = s => { called = true; };
            var bus = new RedisNotificationBus("localhost:6379",
                new InvalidationSettings
                    {InvalidationStrategy = InvalidationStrategyType.External, InvalidationCallback = cb});
            bus.Connection = MockOfConnection.Object;
            var monitor = new RedisChangeMonitor(bus.Notifier, "mykey");
            lcache.Add("mykey", DateTime.UtcNow,
                new CacheItemPolicy {AbsoluteExpiration = DateTime.UtcNow.AddDays(1), ChangeMonitors = {monitor}});

            bus.Start();

            //act
            NotificationEmitter(Constants.DEFAULT_INVALIDATION_CHANNEL, "mykey");

            //assert
            Assert.True(lcache.Contains("mykey"));
            Assert.False(monitor.IsDisposed);
            Assert.True(called);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void RedisNotificationBus_WhenInvalidation_ShouldRemoveFromDefaultCache()
        {
            var lcache = new MemoryCache(Guid.NewGuid().ToString());
            var bus = new RedisNotificationBus("localhost:6379",
                new InvalidationSettings
                    {TargetCache = lcache, InvalidationStrategy = InvalidationStrategyType.AutoCacheRemoval});
            bus.Connection = MockOfConnection.Object;
            var monitor = new RedisChangeMonitor(bus.Notifier, "mykey");
            lcache.Add("mykey", DateTime.UtcNow,
                new CacheItemPolicy {AbsoluteExpiration = DateTime.UtcNow.AddDays(1), ChangeMonitors = {monitor}});

            bus.Start();

            //act
            NotificationEmitter(Constants.DEFAULT_INVALIDATION_CHANNEL, "mykey");

            //assert
            Assert.False(lcache.Contains("mykey"));
            Assert.True(monitor.IsDisposed);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void RedisNotificationBus_WhenInvalidCtorArgs_ShouldNotThrowExceptions()
        {
            var bus = new RedisNotificationBus("fghfgh", new InvalidationSettings());
            Assert.NotNull(bus.Connection);
            Assert.NotNull(bus.Notifier);
            Assert.Equal(MemoryCache.Default, bus.LocalCache);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void RedisNotificationBus_WhenNotify_ShouldPublishAsync()
        {
            var bus = new RedisNotificationBus("localhost:6379", new InvalidationSettings());
            bus.Connection = MockOfConnection.Object;


            var notifyTask = bus.NotifyAsync("mykey");

            Assert.NotNull(notifyTask);
            Assert.Equal(5, notifyTask.Result);
            MockOfConnection.Verify(c => c.PublishAsync(Constants.DEFAULT_INVALIDATION_CHANNEL, "mykey"), Times.Once);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void RedisNotificationBus_WhenStart_ShouldConnectAndSubscribe()
        {
            var bus = new RedisNotificationBus("localhost:6379", new InvalidationSettings());
            bus.Connection = MockOfConnection.Object;

            bus.Start();

            MockOfConnection.Verify(c => c.Connect(), Times.Once);
            MockOfConnection.Verify(
                c => c.Subscribe(Constants.DEFAULT_INVALIDATION_CHANNEL, It.IsAny<Action<RedisChannel, RedisValue>>()),
                Times.Once);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void RedisNotificationBus_WhenStop_ShouldDisconnect()
        {
            var bus = new RedisNotificationBus("localhost:6379", new InvalidationSettings());
            bus.Connection = MockOfConnection.Object;


            var notifyTask = bus.NotifyAsync("mykey");

            Assert.NotNull(notifyTask);
            Assert.Equal(5, notifyTask.Result);
            MockOfConnection.Verify(c => c.PublishAsync(Constants.DEFAULT_INVALIDATION_CHANNEL, "mykey"), Times.Once);
        }
    }
}