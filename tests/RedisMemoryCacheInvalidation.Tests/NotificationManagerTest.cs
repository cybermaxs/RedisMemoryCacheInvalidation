using System.Linq;
using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Core.Interfaces;
using Moq;
using Xunit;

namespace RedisMemoryCacheInvalidation.Tests
{
    public class NotificationManagerTest
    {
        [Fact]
        public void NotificationManager_WhenSubscribed_ShouldPass()
        {
            var mockOfObserver = new Mock<INotificationObserver<string>>();
            var notifier = new NotificationManager();
            var res = notifier.Subscribe("mykey", mockOfObserver.Object);

            Assert.NotNull(res);
            Assert.Equal(1, notifier.SubscriptionsByTopic.Values.Count);
            Assert.True(notifier.SubscriptionsByTopic.Values.SelectMany(e=>e).Contains(mockOfObserver.Object));
            Assert.IsType<Unsubscriber>(res);
        }

        [Fact]
        public void NotificationManager_WhenSubscribedTwice_ShouldBeSubscriberOnlyOnce()
        {
            var mockOfObserver = new Mock<INotificationObserver<string>>();
            var notifier = new NotificationManager();
            var res1 = notifier.Subscribe("mykey", mockOfObserver.Object);
            var res2 = notifier.Subscribe("mykey", mockOfObserver.Object);

            Assert.NotNull(res1);
            Assert.Same(res1, res2);
            Assert.Equal(1, notifier.SubscriptionsByTopic.Values.Count);
            Assert.True(notifier.SubscriptionsByTopic.Values.SelectMany(e => e).Contains(mockOfObserver.Object));
            Assert.IsType<Unsubscriber>(res1);
        }

        [Fact]
        public void NotificationManager_WhenNotifyAll_ShouldPass()
        {
            var mockOfObserver1 = new Mock<INotificationObserver<string>>();
            var mockOfObserver2 = new Mock<INotificationObserver<string>>();
            var notifier = new NotificationManager();
            var res1 = notifier.Subscribe("mykey", mockOfObserver1.Object);
            var res2 = notifier.Subscribe("mykey", mockOfObserver2.Object);

            notifier.Notify("mykey");

            Assert.NotNull(notifier.SubscriptionsByTopic.Values.SelectMany(e => e).Count() == 0); 
        }
    }
}
