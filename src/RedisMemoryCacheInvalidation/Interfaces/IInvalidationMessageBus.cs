using System;
using System.Threading.Tasks;

namespace RedisMemoryCacheInvalidation
{
    public interface IInvalidationMessageBus
    {
        void Notify(string key);
        /// <summary>
        /// Send invalidation message.
        /// Publish an invalidation message to redis for a key.
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="RedisConnectionClosedException">The Redis Connection is not available.</exception>
        /// <returns>Number of subscribers.</returns>
        Task<long> Invalidate(string key);
    }
}
