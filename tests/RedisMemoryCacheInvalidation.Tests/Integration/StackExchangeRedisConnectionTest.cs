using RedisMemoryCacheInvalidation.Redis;
using RedisMemoryCacheInvalidation.Tests.Fixtures;
using System.Threading;
using Xunit;

namespace RedisMemoryCacheInvalidation.Integration.Tests
{
    [Collection("RedisServer")]
    public class StackExchangeRedisConnectionTests
    {
        //private RedisServerFixture redisServer;
        //public StackExchangeRedisConnectionTests(RedisServerFixture redisServer)
        //{
        //    this.redisServer = redisServer;
        //}

        //[Fact]
        //[Trait("Category", "Integration")]
        //public void StackExchangeRedisConnection_ConnectInvalidHost_WhenInvalidHost_ShouldNotBeConnected()
        //{
        //    var cnx = new StackExchangeRedisConnection("pingpong");
        //    Assert.False(cnx.IsConnected);

        //    var connecttask = cnx.ConnectAsync();
        //    connecttask.Wait();

        //    Assert.True(connecttask.IsCompleted);
        //    Assert.False(cnx.IsConnected);
        //}

        //[Fact]
        //[Trait("Category", "Integration")]
        //public void StackExchangeRedisConnection_ConnectInvalidHost_ShouldBeConnected()
        //{
        //    this.redisServer.Reset();

        //    var cnx = new StackExchangeRedisConnection("localhost:6379");
        //    Assert.False(cnx.IsConnected);

        //    var connecttask = cnx.ConnectAsync();
        //    connecttask.Wait();

        //    Assert.True(connecttask.IsCompleted);
        //    Assert.True(cnx.IsConnected);

        //    this.redisServer.Dispose();
        //}

        //[Fact]
        //[Trait("Category", "Integration")]
        //public void StackExchangeRedisConnection_DisconnectWhenConnected_ShouldBeDisconnected()
        //{
        //    RedisServer.Start();

        //    var cnx = new StackExchangeRedisConnection("localhost:6379");
        //    Assert.False(cnx.IsConnected);

        //    var connecttask = cnx.ConnectAsync();
        //    connecttask.Wait();

        //    Assert.True(cnx.IsConnected);

        //    var disconnecttask = cnx.DisconnectAsync();
        //    disconnecttask.Wait();

           
        //    Assert.False(cnx.IsConnected);

        //    RedisServer.Kill();
        //}

        //[Fact]
        //[Trait("Category", "Integration")]
        //public void StackExchangeRedisConnection_WhenRedisFailure_ShouldBeDisconnected()
        //{
        //    RedisServer.Start();

        //    var cnx = new StackExchangeRedisConnection("localhost:6379");
        //    Assert.False(cnx.IsConnected);

        //    var connecttask = cnx.ConnectAsync();
        //    connecttask.Wait();

        //    Assert.True(cnx.IsConnected);

        //    RedisServer.Kill();

        //    //HACK : require to see disconnection
        //    Thread.Sleep(5000);

        //    Assert.False(cnx.IsConnected);
        //}
    }
}
