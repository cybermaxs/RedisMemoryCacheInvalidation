using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Core.Interfaces;
using Moq;
using RedisMemoryCacheInvalidation.Utils;
using Xunit;

namespace RedisMemoryCacheInvalidation.Tests
{
    public class UnsubscriberTest
    {
        [Fact]
        public void Unsubscriber_WhenUnsubscribe_ShouldBeDisposed()
        {
            var mock1 = new Mock<INotificationObserver<string>>();
            var mock2 = new Mock<INotificationObserver<string>>();
            var mock3 = new Mock<INotificationObserver<string>>();

            var obs = new SynchronizedCollection<INotificationObserver<string>> {mock1.Object, mock2.Object, mock3.Object };
            var unsub = new Unsubscriber(obs, mock2.Object);
            unsub.Dispose();

            Assert.Equal(2, obs.Count);
            Assert.False(obs.Contains(mock2.Object));            
        }
    }
}
