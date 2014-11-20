using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Runtime.Caching;

namespace RedisMemoryCacheInvalidation.Tests
{
    [TestClass]
    public class InvalidationManagerTest
    {
        Mock<IRedisNotificationBus> MockOfBus = new Mock<IRedisNotificationBus>();

        [TestInitialize]
        public void TestInitialize()
        {
            InvalidationManager.notificationBus = null;
        }

        #region Configure
        [TestMethod]
        public void Configure_WhenInvalid_ShouldThrowException()
        {
            var configTask = InvalidationManager.ConfigureAsync("dfsdf");
            configTask.Wait();

            Assert.IsTrue(configTask.IsCompleted);
            Assert.IsFalse(InvalidationManager.IsConnected);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Configure_WhenTwice_ShouldThrowException()
        {
            InvalidationManager.ConfigureAsync("dfsdf");
            InvalidationManager.ConfigureAsync("dfsdf");
        }
        #endregion

        #region Invalid Parameters

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateChangeMonitor_WhenNotConfigured_ShouldThrowException()
        {
            InvalidationManager.CreateChangeMonitor("rzer");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateChangeMonitorWithCacheItem_WhenNotConfigured_ShouldThrowException()
        {
            InvalidationManager.CreateChangeMonitor(new CacheItem("rzesdqr"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Invalidate_WhenNotConfigured_ShouldThrowException()
        {
            InvalidationManager.InvalidateAsync("rzaaer");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateChangeMonitor_WhenNull_ShouldThrowException()
        {
            InvalidationManager.CreateChangeMonitor((string)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateChangeMonitorWithCacheItem_WhenNullCacheItem_ShouldThrowException()
        {
            InvalidationManager.CreateChangeMonitor((CacheItem)null);
        }
        #endregion

        [TestMethod]
        public void Invalidate_WhenInvalid_ShouldPublushToRedis()
        {
            InvalidationManager.notificationBus = this.MockOfBus.Object;

            InvalidationManager.InvalidateAsync("mykey");

            this.MockOfBus.Verify(b => b.NotifyAsync("mykey"), Times.Once);
        }
    }
}
