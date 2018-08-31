using Moq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using AutoFixture;
using Xunit;
using RedisMemoryCacheInvalidation.Redis;

namespace RedisMemoryCacheInvalidation.Tests.Redis
{
    public class ExistingRedisConnectionTests
    {
        private Mock<IConnectionMultiplexer> mockOfMux;
        private Mock<ISubscriber> mockOfSubscriber;
        private IRedisConnection cnx;
        private Fixture fixture = new Fixture();

        public ExistingRedisConnectionTests()
        {
            //mock of subscriber
            mockOfSubscriber = new Mock<ISubscriber>();
            mockOfSubscriber.Setup(s => s.UnsubscribeAll(It.IsAny<CommandFlags>()));
            mockOfSubscriber.Setup(s => s.Subscribe(It.IsAny<RedisChannel>(), It.IsAny<Action<RedisChannel, RedisValue>>(), It.IsAny<CommandFlags>()));
            mockOfSubscriber.Setup(s => s.PublishAsync(It.IsAny<RedisChannel>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>())).ReturnsAsync(10L);
            //mock of mux
            mockOfMux = new Mock<IConnectionMultiplexer>();
            mockOfMux.Setup(c => c.IsConnected).Returns(true);
            mockOfMux.Setup(c => c.Close(false));
            mockOfMux.Setup(c => c.GetSubscriber(It.IsAny<object>())).Returns(this.mockOfSubscriber.Object);

            cnx = new ExistingRedisConnnection(mockOfMux.Object);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void WhenNotConnected_ShouldDoNothing()
        {
            this.mockOfMux.Setup(c => c.IsConnected).Returns(false);

            //connected
            var connected = cnx.Connect();
            Assert.False(connected);

            //subscribe
            cnx.Subscribe("channel", (c, v) => { }) ;

            //getconfig
            var config = cnx.GetConfigAsync().Result;
            Assert.Equal<KeyValuePair<string, string>[]>(new KeyValuePair<string, string>[] { }, config);

            //publish
            var published = cnx.PublishAsync("channel", "value").Result;
            Assert.Equal(0L, published);

            cnx.UnsubscribeAll();
            cnx.Disconnect();

            mockOfMux.Verify(c => c.IsConnected, Times.AtLeastOnce);
            mockOfMux.Verify(c => c.GetSubscriber(null), Times.Never);
            mockOfMux.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void WhenConnect_ShouldCheck_MuxIsConnected()
        {
            var connected = cnx.Connect();
            Assert.True(connected);

            mockOfMux.Verify(c => c.IsConnected, Times.Once);

            Assert.True(cnx.IsConnected);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void WhenDisconnect_ShouldUnsubscribeAll()
        {
            cnx.Disconnect();

            mockOfSubscriber.Verify(m => m.UnsubscribeAll(It.IsAny<CommandFlags>()), Times.Once);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void WhenPublishAsync_ShouldPublishMessages()
        {
            var channel = fixture.Create<string>();
            var value = fixture.Create<string>();

            var published = cnx.PublishAsync(channel, value).Result;

            Assert.Equal(10L, published);
            mockOfSubscriber.Verify(m => m.PublishAsync(channel, value, It.IsAny<CommandFlags>()), Times.Once);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void WhenSubscribe_ShouldSubscribe()
        {
            var channel = fixture.Create<string>();
            Action<RedisChannel, RedisValue> action = (c, v) => { };

            cnx.Subscribe(channel, action);

            mockOfSubscriber.Verify(s => s.Subscribe(channel, action, It.IsAny<CommandFlags>()), Times.Once);
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void WhenUnsubscribeAll_ShouldUnsubscribeAll()
        {
            cnx.UnsubscribeAll();

            mockOfSubscriber.Verify(s => s.UnsubscribeAll(It.IsAny<CommandFlags>()), Times.Once);
        }
    }
}
