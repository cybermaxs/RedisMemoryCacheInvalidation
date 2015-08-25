using Moq;
using System;
using System.Runtime.Caching;
using Xunit;

namespace RedisMemoryCacheInvalidation.Tests
{
    public class InvalidationManagerTests
    {
        private readonly Mock<IRedisNotificationBus> mockOfBus;


        public InvalidationManagerTests()
        {
            InvalidationManager.notificationBus = null;
            mockOfBus = new Mock<IRedisNotificationBus>();
        }

        #region Configure
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
        #endregion

        #region Invalid Parameters

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
        #endregion

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void Invalidate_WhenInvalid_ShouldPublushToRedis()
        {
            InvalidationManager.notificationBus = this.mockOfBus.Object;
            InvalidationManager.InvalidateAsync("mykey");
            this.mockOfBus.Verify(b => b.NotifyAsync("mykey"), Times.Once);
        }
    }
}
