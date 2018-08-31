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

        public bool IsConnected
        {
            get { return multiplexer != null && multiplexer.IsConnected; }
        }

        public void Subscribe(string channel, Action<RedisChannel, RedisValue> handler)
        {
            if (IsConnected)
            {
                var subscriber = multiplexer.GetSubscriber();
                subscriber.Subscribe(channel, handler);
            }
        }

        public void UnsubscribeAll()
        {
            if (IsConnected)
                multiplexer.GetSubscriber().UnsubscribeAll();
        }

        public Task<long> PublishAsync(string channel, string value)
        {
            if (IsConnected)
            {
                return multiplexer.GetSubscriber().PublishAsync(channel, value);
            }
            else
                return TaskCache.FromResult(0L);
        }
        public Task<KeyValuePair<string, string>[]> GetConfigAsync()
        {
            if (IsConnected)
            {
                var server = GetServer();
                return server.ConfigGetAsync();
            }
            else
                return TaskCache.FromResult(new KeyValuePair<string, string>[] { });
        }

        protected IServer GetServer()
        {
            var endpoints = multiplexer.GetEndPoints();
            IServer result = null;
            foreach (var endpoint in endpoints)
            {
                var server = multiplexer.GetServer(endpoint);
                if (server.IsSlave || !server.IsConnected) continue;
                if (result != null) throw new InvalidOperationException("Requires exactly one master endpoint (found " + server.EndPoint + " and " + result.EndPoint + ")");
                result = server;
            }
            if (result == null) throw new InvalidOperationException("Requires exactly one master endpoint (found none)");
            return result;
        }

        public abstract bool Connect();

        public abstract void Disconnect();
    }
}
