using Moq;
using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Core.Interfaces;
using RedisMemoryCacheInvalidation.Monitor;
using System;
using Xunit;

namespace RedisMemoryCacheInvalidation.Tests
{
    public class RedisChangeMonitorTest
    {
        public const string notifKey = "unittestkey";

        Mock<INotificationManager<string>> MockOfBus;
        Mock<IDisposable> MockOfDispose;

        INotificationManager<string> Bus { get { return MockOfBus.Object; } }
        
        public RedisChangeMonitorTest()
        {
            this.MockOfDispose = new Mock<IDisposable>();
            this.MockOfBus = new Mock<INotificationManager<string>>();
            this.MockOfBus.Setup(t => t.Subscribe(It.IsAny<string>(), It.IsAny<INotificationObserver<string>>())).Returns(this.MockOfDispose.Object);
        }

        #region Tests
        [Fact]
        public void RedisChangeMonitor_WhenCtorWithoutBusBadArgs_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => { RedisChangeMonitor monitor = new RedisChangeMonitor(null, notifKey); });
            Assert.Throws<ArgumentNullException>(() => { RedisChangeMonitor monitor = new RedisChangeMonitor(this.Bus, null); });
        }

        [Fact]
        public void RedisChangeMonitor_WhenCtor_ShouldHaveUniqueId()
        {
            var monitor1 = new RedisChangeMonitor(this.Bus, notifKey);
            Assert.NotNull(monitor1);
            Assert.True(monitor1.UniqueId.Length > 0);

            var monitor2 = new RedisChangeMonitor(this.Bus, notifKey);
            Assert.NotNull(monitor2);
            Assert.True(monitor2.UniqueId.Length > 0);

            Assert.NotSame(monitor1.UniqueId, monitor2.UniqueId);
        }

        [Fact]
        public void RedisChangeMonitor_WhenCtor_ShouldBeRegistered()
        {
            var monitor = new RedisChangeMonitor(this.Bus, notifKey);
            this.MockOfBus.Verify(e => e.Subscribe(notifKey, monitor), Times.Once);
        }

        [Fact]
        public void RedisChangeMonitor_WhenExceptioninCtor_ShouldBeDisposed()
        {
            this.MockOfBus.Setup(e => e.Subscribe(It.IsAny<string>(), It.IsAny<INotificationObserver<string>>())).Throws<InvalidOperationException>();
            RedisChangeMonitor monitor = new RedisChangeMonitor(this.Bus, notifKey);

            Assert.True(monitor.IsDisposed);
        }

        [Fact]
        public void RedisChangeMonitor_WhenChanged_ShouldBeDisposed()
        {
            RedisChangeMonitor monitor = new RedisChangeMonitor(this.Bus, notifKey);
            monitor.Notify(notifKey);

            Assert.True(monitor.IsDisposed);

            this.MockOfBus.Verify(e => e.Subscribe(notifKey, monitor), Times.Once);
            this.MockOfDispose.Verify(e => e.Dispose(), Times.Once);
        }
        #endregion

    }
}
