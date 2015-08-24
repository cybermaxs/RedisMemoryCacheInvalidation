using Moq;
using RedisMemoryCacheInvalidation.Redis;
using StackExchange.Redis;
using Xunit;

namespace RedisMemoryCacheInvalidation.Tests.Redis
{
    public class RedisConnectionFactoryTests
    {
        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void WhenNewWithConfigOptions_Should_Create_StandaloneRedisConnection()
        {
            var cnx = RedisConnectionFactory.New(new ConfigurationOptions());

            Assert.IsType<StandaloneRedisConnection>(cnx);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void WhenNewWithMux_Should_Create_ExistingRedisConnection()
        {
            var mockOfMux = new Mock<IConnectionMultiplexer>();
            var cnx = RedisConnectionFactory.New(mockOfMux.Object);

            Assert.IsType<ExistingRedisConnnection>(cnx);
        }
    }
}
