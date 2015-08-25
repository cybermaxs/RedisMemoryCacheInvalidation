using System.Linq;
using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Core.Interfaces;
using Moq;
using Xunit;
using Ploeh.AutoFixture;

namespace RedisMemoryCacheInvalidation.Tests.Core
{
    public class NotificationManagerTests
    {
        private Fixture fixture = new Fixture();
        private readonly string topciKey;
        public NotificationManagerTests()
        {
            topciKey = fixture.Create<string>();
        }
        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void WhenSubscribed_ShouldPass()
        {
            var mockOfObserver = new Mock<INotificationObserver<string>>();
            var notifier = new NotificationManager();
            var res = notifier.Subscribe(topciKey, mockOfObserver.Object);

            Assert.NotNull(res);
            Assert.Equal(1, notifier.SubscriptionsByTopic.Values.Count);
            Assert.True(notifier.SubscriptionsByTopic.Values.SelectMany(e=>e).Contains(mockOfObserver.Object));
            Assert.IsType<Unsubscriber>(res);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void WhenSubscribedTwice_ShouldBeSubscriberOnlyOnce()
        {
            var mockOfObserver = new Mock<INotificationObserver<string>>();
            var notifier = new NotificationManager();
            var res1 = notifier.Subscribe(topciKey, mockOfObserver.Object);
            var res2 = notifier.Subscribe(topciKey, mockOfObserver.Object);

            Assert.NotNull(res1);
            Assert.NotSame(res1, res2);
            Assert.Equal(1, notifier.SubscriptionsByTopic.Values.Count);
            Assert.True(notifier.SubscriptionsByTopic.Values.SelectMany(e => e).Contains(mockOfObserver.Object));
            Assert.IsType<Unsubscriber>(res1);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void WhenSameTopic_ShouldNotifyAll()
        {
            var mockOfObserver1 = new Mock<INotificationObserver<string>>();
            var mockOfObserver2 = new Mock<INotificationObserver<string>>();
            var notifier = new NotificationManager();
            var res1 = notifier.Subscribe(topciKey, mockOfObserver1.Object);
            var res2 = notifier.Subscribe(topciKey, mockOfObserver2.Object);

            notifier.Notify(topciKey);

            Assert.NotNull(notifier.SubscriptionsByTopic.Values.SelectMany(e => e).Count() == 0); 
        }
    }
}
