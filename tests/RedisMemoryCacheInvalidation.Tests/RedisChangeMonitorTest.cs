using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisMemoryCacheInvalidation.Tests.Helper;

namespace RedisMemoryCacheInvalidation.Tests
{
    [TestClass]
    public class RedisChangeMonitorTest
    {
        public const string notifKey = "unittestkey";

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
            RedisChangeMonitor monitor = new RedisChangeMonitor(new FakeObservable(), null);
        }

        [TestMethod]
        public void RedisChangeMonitor_WhenCtor_ShouldHaveUniqueId()
        {
            RedisChangeMonitor monitor1 = new RedisChangeMonitor(new FakeObservable(), notifKey);
            Assert.IsNotNull(monitor1);
            Assert.IsTrue(monitor1.UniqueId.Length > 0);

            RedisChangeMonitor monitor2 = new RedisChangeMonitor(new FakeObservable(), notifKey);
            Assert.IsNotNull(monitor2);
            Assert.IsTrue(monitor2.UniqueId.Length > 0);

            Assert.AreNotEqual(monitor1.UniqueId, monitor2.UniqueId);
        }

        [TestMethod]
        public void RedisChangeMonitor_WhenCtor_ShouldBeRegistered()
        {
            FakeObservable bus = new FakeObservable();
            RedisChangeMonitor monitor = new RedisChangeMonitor(bus, notifKey);

            Assert.IsNotNull(bus.observers);
            Assert.AreEqual(1, bus.observers.Count);
            Assert.AreSame(monitor, bus.observers[0]);
        }

        [TestMethod]
        public void RedisChangeMonitor_WhenNotified_ShouldChangedAndDisposedAndUnregistered()
        {
            FakeObservable obs = new FakeObservable();

            RedisChangeMonitor monitor = new RedisChangeMonitor(obs, notifKey);
            obs.Notify(notifKey);

            Assert.IsNotNull(monitor);
            Assert.IsTrue(monitor.IsDisposed);
            Assert.IsTrue(monitor.HasChanged);

            Assert.IsNotNull(obs.observers);
            Assert.AreEqual(0, obs.observers.Count);
        }
    }
}
