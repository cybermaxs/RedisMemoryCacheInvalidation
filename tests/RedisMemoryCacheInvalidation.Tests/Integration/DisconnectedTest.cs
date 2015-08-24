using Xunit;

namespace RedisMemoryCacheInvalidation.Tests.Integration
{
    public class DisconnectedTest
    {
        [Fact]
        [Trait("Category","Integration")]
        public void TodoTest()
        {
            //test more disconnected scenarios
            InvalidationManager.Configure("blabblou", new InvalidationSettings());
            var t = InvalidationManager.InvalidateAsync("mykey");

            Assert.NotNull(t);
            Assert.True(t.IsFaulted);
        }
    }
}
