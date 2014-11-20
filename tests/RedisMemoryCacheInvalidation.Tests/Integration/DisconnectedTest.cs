using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisMemoryCacheInvalidation.Tests.Helper;

namespace RedisMemoryCacheInvalidation.Tests.Integration
{
    [TestClass]
    public class DisconnectedTest
    {
        [TestMethod]
        [TestCategory("Integration")]
        public void TodoTest()
        {
            //test more disconnected scenarios
            InvalidationManager.ConfigureAsync("blabblou").Wait();
            var t = InvalidationManager.InvalidateAsync("mykey");

            Assert.IsNotNull(t);
            Assert.IsTrue(t.IsFaulted);
        }
    }
}
