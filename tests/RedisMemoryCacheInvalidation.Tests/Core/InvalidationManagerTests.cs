using Moq;
using System;
using System.Runtime.Caching;
using Xunit;

namespace RedisMemoryCacheInvalidation.Tests
{
    public class InvalidationManagerTests
    {
        private readonly Mock<IRedisNotificationBus> _mockOfBus;

        public InvalidationManagerTests()
        {
            InvalidationManager.NotificationBus = null;
            _mockOfBus = new Mock<IRedisNotificationBus>();
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void Configure_WhenInvalid_ShouldThrowException()
        {
            InvalidationManager.Configure("dfsdf", new InvalidationSettings());

            Assert.False(InvalidationManager.IsConnected);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void Configure_WhenTwice_ShouldNotThrowException()
        {
            InvalidationManager.Configure("dfsdf", new InvalidationSettings());
            InvalidationManager.Configure("dfsdf", new InvalidationSettings());
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void CreateChangeMonitorBadArgs_ShouldThrowException()
        {
            Assert.Throws<InvalidOperationException>(() => { InvalidationManager.CreateChangeMonitor("rzer"); });
            Assert.Throws<InvalidOperationException>(() => { InvalidationManager.CreateChangeMonitor(new CacheItem("rzesdqr")); });
            Assert.Throws<InvalidOperationException>(() => { InvalidationManager.InvalidateAsync("rzaaer"); });
            Assert.Throws<ArgumentNullException>(() => { InvalidationManager.CreateChangeMonitor((string)null); });
            Assert.Throws<ArgumentNullException>(() => { InvalidationManager.CreateChangeMonitor((CacheItem)null); });
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void Invalidate_WhenInvalid_ShouldPublishToRedis()
        {
            InvalidationManager.NotificationBus = this._mockOfBus.Object;
            InvalidationManager.InvalidateAsync("mykey");
            this._mockOfBus.Verify(b => b.NotifyAsync("mykey"), Times.Once);
        }
    }
}
