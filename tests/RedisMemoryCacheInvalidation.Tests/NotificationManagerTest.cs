using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Core.Interfaces;
using Moq;

namespace RedisMemoryCacheInvalidation.Tests
{
    [TestClass]
    public class NotificationManagerTest
    {

        [TestInitialize]
        public void TestInit()
        {
            
        }

        [TestMethod]
        public void NotificationManager_WhenSubscribed_ShouldPass()
        {
            Mock<INotificationObserver<string>> mockOfObserver = new Mock<INotificationObserver<string>>();
            var notifier = new NotificationManager();
            var res = notifier.Subscribe("mykey", mockOfObserver.Object);

            Assert.IsNotNull(res);
            Assert.AreEqual(1, notifier.SubscriptionsByTopic.Values.Count);
            Assert.IsTrue(notifier.SubscriptionsByTopic.Values.SelectMany(e=>e).Contains(mockOfObserver.Object));
            Assert.IsInstanceOfType(res, typeof(Unsubscriber));
        }

        [TestMethod]
        public void NotificationManager_WhenSubscribedTwice_ShouldBeSubscriberOnlyOnce()
        {
            Mock<INotificationObserver<string>> mockOfObserver = new Mock<INotificationObserver<string>>();
            var notifier = new NotificationManager();
            var res1 = notifier.Subscribe("mykey", mockOfObserver.Object);
            var res2 = notifier.Subscribe("mykey", mockOfObserver.Object);

            Assert.IsNotNull(res1);
            Assert.AreNotSame(res1, res2);
            Assert.AreEqual(1, notifier.SubscriptionsByTopic.Values.Count);
            Assert.IsTrue(notifier.SubscriptionsByTopic.Values.SelectMany(e => e).Contains(mockOfObserver.Object));
            Assert.IsInstanceOfType(res1, typeof(Unsubscriber));
        }

        [TestMethod]
        public void NotificationManager_WhenNotifyAll_ShouldPass()
        {
            Mock<INotificationObserver<string>> mockOfObserver1 = new Mock<INotificationObserver<string>>();
            Mock<INotificationObserver<string>> mockOfObserver2 = new Mock<INotificationObserver<string>>();
            var notifier = new NotificationManager();
            var res1 = notifier.Subscribe("mykey", mockOfObserver1.Object);
            var res2 = notifier.Subscribe("mykey", mockOfObserver2.Object);

            notifier.Notify("mykey");

            Assert.IsNotNull(notifier.SubscriptionsByTopic.Values.SelectMany(e => e).Count() == 0); 
        }
    }
}
