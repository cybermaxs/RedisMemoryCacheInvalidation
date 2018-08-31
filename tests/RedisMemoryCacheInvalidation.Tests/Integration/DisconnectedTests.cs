using Xunit;

namespace RedisMemoryCacheInvalidation.Tests.Integration
{
    public class DisconnectedTests
    {
        public DisconnectedTests()
        {
            InvalidationManager.NotificationBus = null;
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.IntegrationTestCategory)]
        public void InvalidHost_ShouldNotBeConnected()
        {
            //test more disconnected scenarios
            InvalidationManager.Configure("blabblou", new InvalidationSettings());
            Assert.False(InvalidationManager.IsConnected);

            var published = InvalidationManager.InvalidateAsync("mykey").Result;
            Assert.Equal(0L, published);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.IntegrationTestCategory)]
        public void WhenNotConnected_ShouldNotPublishMessages()
        {
            //test more disconnected scenarios
            InvalidationManager.Configure("blabblou", new InvalidationSettings());

            var published = InvalidationManager.InvalidateAsync("mykey").Result;
            Assert.Equal(0L, published);
        }
    }
}
