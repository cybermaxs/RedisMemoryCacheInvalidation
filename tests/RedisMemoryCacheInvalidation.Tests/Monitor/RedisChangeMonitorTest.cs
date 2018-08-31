using Moq;
using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Core.Interfaces;
using RedisMemoryCacheInvalidation.Monitor;
using System;
using AutoFixture;
using Xunit;

namespace RedisMemoryCacheInvalidation.Tests.Monitor
{
    public class RedisChangeMonitorTest
    {
        private readonly string notifKey;

        private readonly Fixture fixture = new Fixture();
        private readonly Mock<INotificationManager<string>> mockOfBus;
        private readonly Mock<IDisposable> mockOfDispose;

        INotificationManager<string> Bus { get { return mockOfBus.Object; } }

        public RedisChangeMonitorTest()
        {
            mockOfDispose = new Mock<IDisposable>();
            mockOfBus = new Mock<INotificationManager<string>>();
            mockOfBus.Setup(t => t.Subscribe(It.IsAny<string>(), It.IsAny<INotificationObserver<string>>())).Returns(this.mockOfDispose.Object);

            notifKey = fixture.Create<string>();
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void WhenCtorWithoutBusBadArgs_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => { var monitor = new RedisChangeMonitor(null, notifKey); });
            Assert.Throws<ArgumentNullException>(() => { var monitor = new RedisChangeMonitor(this.Bus, null); });
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void WhenCtor_ShouldHaveUniqueId()
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
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void WhenCtor_ShouldBeRegistered()
        {
            var monitor = new RedisChangeMonitor(this.Bus, notifKey);
            this.mockOfBus.Verify(e => e.Subscribe(notifKey, monitor), Times.Once);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void WhenExceptionInCtor_ShouldBeDisposed()
        {
            this.mockOfBus.Setup(e => e.Subscribe(It.IsAny<string>(), It.IsAny<INotificationObserver<string>>())).Throws<InvalidOperationException>();
            var monitor = new RedisChangeMonitor(this.Bus, notifKey);

            Assert.True(monitor.IsDisposed);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void WhenChanged_ShouldBeDisposed()
        {
            var monitor = new RedisChangeMonitor(this.Bus, notifKey);
            monitor.Notify(notifKey);

            Assert.True(monitor.IsDisposed);

            this.mockOfBus.Verify(e => e.Subscribe(notifKey, monitor), Times.Once);
            this.mockOfDispose.Verify(e => e.Dispose(), Times.Once);
        }
    }
}
