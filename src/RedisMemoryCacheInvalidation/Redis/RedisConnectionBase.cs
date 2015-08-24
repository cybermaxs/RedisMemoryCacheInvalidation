using RedisMemoryCacheInvalidation.Utils;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedisMemoryCacheInvalidation.Redis
{
    internal abstract class RedisConnectionBase : IRedisConnection
    {
        protected IConnectionMultiplexer multiplexer;

        #region IRedisConnection
        public bool IsConnected
        {
            get { return this.multiplexer != null && this.multiplexer.IsConnected; }
        }

        public void Subscribe(string channel, Action<RedisChannel, RedisValue> handler)
        {
            if (this.IsConnected)
            {
                var subscriber = this.multiplexer.GetSubscriber();
                subscriber.Subscribe(channel, handler);
            }
        }

        public void UnsubscribeAll()
        {
            if (this.IsConnected)
                this.multiplexer.GetSubscriber().UnsubscribeAll();
        }

        public Task<long> PublishAsync(string channel, string value)
        {
            if (this.IsConnected)
            {
                return this.multiplexer.GetSubscriber().PublishAsync(channel, value);
            }
            else
                return TaskCache.FromResult(0L);
        }
        public Task<KeyValuePair<string, string>[]> GetConfigAsync()
        {
            if (this.IsConnected)
            {
                var server = this.GetServer();
                return server.ConfigGetAsync();
            }
            else
                return TaskCache.FromResult(new KeyValuePair<string, string>[] { });
        }
        #endregion

        #region privates
        protected IServer GetServer()
        {
            var endpoints = this.multiplexer.GetEndPoints();
            IServer result = null;
            foreach (var endpoint in endpoints)
            {
                var server = this.multiplexer.GetServer(endpoint);
                if (server.IsSlave || !server.IsConnected) continue;
                if (result != null) throw new InvalidOperationException("Requires exactly one master endpoint (found " + server.EndPoint + " and " + result.EndPoint + ")");
                result = server;
            }
            if (result == null) throw new InvalidOperationException("Requires exactly one master endpoint (found none)");
            return result;
        }

        public abstract bool Connect();

        public abstract void Disconnect();
        #endregion
    }
}
