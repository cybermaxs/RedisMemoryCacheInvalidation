using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedisMemoryCacheInvalidation.Redis
{
    public interface IRedisConnection
    {
        bool IsConnected { get; }
        Task<bool> ConnectAsync();
        Task DisconnectAsync();
        Task<KeyValuePair<string, string>[]> GetConfigAsync();
        Task SubscribeAsync(string channel, Action<string, string> handler);
        Task UnsubscribeAllAsync();
        Task<long> PublishAsync(string channel, string value);
    }
}
