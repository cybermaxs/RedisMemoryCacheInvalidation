using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Core.Interfaces;
using RedisMemoryCacheInvalidation.Monitor;
using System;

namespace RedisMemoryCacheInvalidation.Tests
{
    [TestClass]
    public class RedisChangeMonitorTest
    {
        public const string notifKey = "unittestkey";

        Mock<INotificationManager<string>> MockOfBus;
        Mock<IDisposable> MockOfDispose;

        INotificationManager<string> Bus { get { return MockOfBus.Object; } }
        

        [TestInitialize]
        public void TestInit()
        {
            this.MockOfDispose = new Mock<IDisposable>();
            this.MockOfBus = new Mock<INotificationManager<string>>();
            this.MockOfBus.Setup(t => t.Subscribe(It.IsAny<string>(), It.IsAny<INotificationObserver<string>>())).Returns(this.MockOfDispose.Object);
        }

        #region Tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RedisChangeMonitor_WhenCtorWithoutBus_ShouldThrowArgumentNullException()
        {
            RedisChangeMonitor monitor = new RedisChangeMonitor(null, notifKey);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RedisChangeMonitor_WhenCtorWithoutKey_ShouldThrowArgumentNullException()
        {
            RedisChangeMonitor monitor = new RedisChangeMonitor(this.Bus, null);
        }

        [TestMethod]
        public void RedisChangeMonitor_WhenCtor_ShouldHaveUniqueId()
        {
            RedisChangeMonitor monitor1 = new RedisChangeMonitor(this.Bus, notifKey);
            Assert.IsNotNull(monitor1);
            Assert.IsTrue(monitor1.UniqueId.Length > 0);

            RedisChangeMonitor monitor2 = new RedisChangeMonitor(this.Bus, notifKey);
            Assert.IsNotNull(monitor2);
            Assert.IsTrue(monitor2.UniqueId.Length > 0);

            Assert.AreNotEqual(monitor1.UniqueId, monitor2.UniqueId);
        }

        [TestMethod]
        public void RedisChangeMonitor_WhenCtor_ShouldBeRegistered()
        {
            RedisChangeMonitor monitor = new RedisChangeMonitor(this.Bus, notifKey);
            this.MockOfBus.Verify(e => e.Subscribe(notifKey, monitor), Times.Once);
        }

        [TestMethod]
        public void RedisChangeMonitor_WhenExceptioninCtor_ShouldBeDisposed()
        {
            this.MockOfBus.Setup(e => e.Subscribe(It.IsAny<string>(), It.IsAny<INotificationObserver<string>>())).Throws<InvalidOperationException>();
            RedisChangeMonitor monitor = new RedisChangeMonitor(this.Bus, notifKey);

            Assert.IsTrue(monitor.IsDisposed);
        }

        [TestMethod]
        public void RedisChangeMonitor_WhenChanged_ShouldBeDisposed()
        {
            RedisChangeMonitor monitor = new RedisChangeMonitor(this.Bus, notifKey);
            monitor.Notify(notifKey);

            Assert.IsTrue(monitor.IsDisposed);

            this.MockOfBus.Verify(e => e.Subscribe(notifKey, monitor), Times.Once);
            this.MockOfDispose.Verify(e => e.Dispose(), Times.Once);
        }
        #endregion

    }
}
