using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Core.Interfaces;
using Moq;
using RedisMemoryCacheInvalidation.Utils;

namespace RedisMemoryCacheInvalidation.Tests
{
    [TestClass]
    public class UnsubscriberTest
    {
        [TestMethod]
        public void Unsubscriber_WhenUnsubscribe_ShouldBeDisposed()
        {
            Mock<INotificationObserver<string>> mock1 = new Mock<INotificationObserver<string>>();
            Mock<INotificationObserver<string>> mock2 = new Mock<INotificationObserver<string>>();
            Mock<INotificationObserver<string>> mock3 = new Mock<INotificationObserver<string>>();

            var obs = new SynchronizedCollection<INotificationObserver<string>>() {mock1.Object, mock2.Object, mock3.Object };
            Unsubscriber unsub = new Unsubscriber(obs, mock2.Object);
            unsub.Dispose();

            Assert.AreEqual(2, obs.Count);
            Assert.IsFalse(obs.Contains(mock2.Object));            
        }
    }
}
