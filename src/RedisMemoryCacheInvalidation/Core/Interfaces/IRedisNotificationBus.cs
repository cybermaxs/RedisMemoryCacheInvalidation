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
        InvalidationStrategyType InvalidationStrategy { get; }
        MemoryCache LocalCache { get; }
        INotificationManager<string> Notifier { get;  }
        Task<long> NotifyAsync(string key);
        void Start();
        void Stop();
    }
}
