using System.Threading.Tasks;

namespace RedisMemoryCacheInvalidation
{
    public interface IInvalidationMessageBus
    {
        /// <summary>
        /// Notify subscribers though Redis that something has changed.
        /// Send invalidation message to Redis for the key.
        /// </summary>
        /// <param name="key">Message</param>
        /// <exception cref="RedisConnectionClosedException">The Redis Connection is not available.</exception>
        /// <returns>Number of subscribers.</returns>
        Task<long> Notify(string key);
        /// <summary>
        /// Local Invalidation.
        /// Publish an invalidation message to redis for a key.
        /// </summary>
        /// <param name="key">Message to invalidate</param>
        void Invalidate(string key);
    }
}
