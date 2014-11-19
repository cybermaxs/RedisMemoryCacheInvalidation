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
        InvalidationStrategy InvalidationStrategy { get; set; }
        MemoryCache LocalCache { get; }
        INotificationManager<string> Notifier { get; set; }
        Task<long> Notify(string key);
        Task Start();
        Task Stop();
    }
}
