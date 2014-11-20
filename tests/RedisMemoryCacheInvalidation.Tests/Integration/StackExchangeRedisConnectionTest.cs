using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisMemoryCacheInvalidation.Redis;
using RedisMemoryCacheInvalidation.Tests.Helper;
using System.Threading;

namespace RedisMemoryCacheInvalidation.Integration.Tests
{
    [TestClass]
    public class StackExchangeRedisConnectionTest
    {
        [TestMethod]
        [TestCategory("Integration")]
        public void StackExchangeRedisConnection_ConnectInvalidHost_WhenInvalidHost_ShouldNotBeConnected()
        {
            StackExchangeRedisConnection cnx = new StackExchangeRedisConnection("pingpong");
            Assert.IsFalse(cnx.IsConnected);

            var connecttask = cnx.ConnectAsync();
            connecttask.Wait();

            Assert.IsTrue(connecttask.IsCompleted);
            Assert.IsFalse(cnx.IsConnected);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void StackExchangeRedisConnection_ConnectInvalidHost_ShouldBeConnected()
        {
            RedisServer.Start();

            StackExchangeRedisConnection cnx = new StackExchangeRedisConnection("localhost:6379");
            Assert.IsFalse(cnx.IsConnected);

            var connecttask = cnx.ConnectAsync();
            connecttask.Wait();

            Assert.IsTrue(connecttask.IsCompleted);
            Assert.IsTrue(cnx.IsConnected);

            RedisServer.Kill();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void StackExchangeRedisConnection_DisconnectWhenConnected_ShouldBeDisconnected()
        {
            RedisServer.Start();

            StackExchangeRedisConnection cnx = new StackExchangeRedisConnection("localhost:6379");
            Assert.IsFalse(cnx.IsConnected);

            var connecttask = cnx.ConnectAsync();
            connecttask.Wait();

            Assert.IsTrue(cnx.IsConnected);

            var disconnecttask = cnx.DisconnectAsync();
            disconnecttask.Wait();

           
            Assert.IsFalse(cnx.IsConnected);

            RedisServer.Kill();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void StackExchangeRedisConnection_WhenRedisFailure_ShouldBeDisconnected()
        {
            RedisServer.Start();

            StackExchangeRedisConnection cnx = new StackExchangeRedisConnection("localhost:6379");
            Assert.IsFalse(cnx.IsConnected);

            var connecttask = cnx.ConnectAsync();
            connecttask.Wait();

            Assert.IsTrue(cnx.IsConnected);

            RedisServer.Kill();

            //HACK : require to see disconnection
            Thread.Sleep(5000);

            Assert.IsFalse(cnx.IsConnected);
        }
    }
}
