using Moq;
using System;
using System.Runtime.Caching;
using Xunit;

namespace RedisMemoryCacheInvalidation.Tests
{
    public class InvalidationManagerTests
    {
        Mock<IRedisNotificationBus> MockOfBus = new Mock<IRedisNotificationBus>();
        

            public InvalidationManagerTests()
        {
            InvalidationManager.notificationBus = null;
        }

        #region Configure
        [Fact]
        public void Configure_WhenInvalid_ShouldThrowException()
        {
            InvalidationManager.Configure("dfsdf", new InvalidationSettings());

            Assert.False(InvalidationManager.IsConnected);
        }

        [Fact]
        public void Configure_WhenTwice_ShouldThrowException()
        {
            //double configuration
            Assert.Throws<InvalidOperationException>(() => {
                InvalidationManager.Configure("dfsdf", new InvalidationSettings());
                InvalidationManager.Configure("dfsdf", new InvalidationSettings());
            });
        }
        #endregion

        #region Invalid Parameters

        [Fact]
        public void CreateChangeMonitorBadArgs_ShouldThrowException()
        {
            //TODO
            Assert.Throws<InvalidOperationException>(() => { InvalidationManager.CreateChangeMonitor("rzer"); });

            Assert.Throws<InvalidOperationException>(() => { InvalidationManager.CreateChangeMonitor(new CacheItem("rzesdqr")); });
            Assert.Throws<InvalidOperationException>(() => { InvalidationManager.InvalidateAsync("rzaaer"); });
            Assert.Throws<ArgumentNullException>(() => { InvalidationManager.CreateChangeMonitor((string)null); });
            Assert.Throws<ArgumentNullException>(() => { InvalidationManager.CreateChangeMonitor((CacheItem)null); });
        }
        #endregion

        [Fact]
        public void Invalidate_WhenInvalid_ShouldPublushToRedis()
        {
            InvalidationManager.notificationBus = this.MockOfBus.Object;

            InvalidationManager.InvalidateAsync("mykey");

            this.MockOfBus.Verify(b => b.NotifyAsync("mykey"), Times.Once);
        }
    }
}
