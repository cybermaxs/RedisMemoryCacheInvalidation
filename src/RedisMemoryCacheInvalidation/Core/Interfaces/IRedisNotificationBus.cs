using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Redis;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace RedisMemoryCacheInvalidation
{
    public interface IRedisNotificationBus
    {
        IRedisConnection Connection { get; }
        void Dispose();
        InvalidationStrategyType InvalidationStrategy { get; set; }
        MemoryCache LocalCache { get; }
        INotificationManager<string> Notifier { get; set; }
        Task<long> NotifyAsync(string key);
        Task StartAsync();
        Task StopAsync();
    }
}
