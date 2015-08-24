using RedisMemoryCacheInvalidation.Redis;
using StackExchange.Redis;
using Xunit;

namespace RedisMemoryCacheInvalidation.Tests.Redis
{
    public class StandaloneRedisConnectionTests
    {
        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void WhenInvalidHost_Should_Not_Be_Connected()
        {
            var options = ConfigurationOptions.Parse("local:6379");
            var cnx = new StandaloneRedisConnection(options);

            var connected = cnx.Connect();

            Assert.False(connected);
            Assert.False(cnx.IsConnected);
        }
    }
}
