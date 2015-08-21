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
            InvalidationManager.ConfigureAsync("blabblou").Wait();
            var t = InvalidationManager.InvalidateAsync("mykey");

            Assert.NotNull(t);
            Assert.True(t.IsFaulted);
        }
    }
}
