using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisMemoryCacheInvalidation.Tests.Helper;

namespace RedisMemoryCacheInvalidation.Tests
{
    [TestClass]
    public class RedisInvalidationMessageBusTest
    {
        private const string UnitTestTopic="mytesttopic";

        [TestMethod]
        public void Bus_Topic_Subscription()
        {
            var bus = new RedisInvalidationMessageBus(new RedisConnectionInfo());

            var observer1 = new FakeObserver();
            bus.Subscribe(UnitTestTopic, observer1);

            var observer2 = new FakeObserver();
            bus.Subscribe(UnitTestTopic, observer2);

            var observer3 = new FakeObserver();
            bus.Subscribe(UnitTestTopic+"2", observer3);

            var observer4 = new FakeObserver();
            bus.Subscribe(UnitTestTopic+"4", observer4);

            bus.Notify(UnitTestTopic);

            Assert.IsTrue(observer1.NextCalled);
            Assert.AreEqual(UnitTestTopic, observer1.NextTopic);
            Assert.IsTrue(observer2.NextCalled);
            Assert.AreEqual(UnitTestTopic, observer2.NextTopic);

            Assert.IsFalse(observer3.NextCalled);
            Assert.IsFalse(observer4.NextCalled);
        }
    }
}
