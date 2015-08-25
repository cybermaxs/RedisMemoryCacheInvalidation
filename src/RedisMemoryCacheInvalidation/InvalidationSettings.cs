using System;
using System.Runtime.Caching;

namespace RedisMemoryCacheInvalidation
{
    public class InvalidationSettings
    {
        /// <summary>
        /// How to process invalidation message (remove from cache, invoke callback, notify change monitor, ...)
        /// </summary>
        public InvalidationStrategyType InvalidationStrategy { get; set; }
        /// <summary>
        /// Target MemoryCache when InvalidationStrategy=AutoCacheRemoval
        /// </summary>
        public MemoryCache TargetCache { get; set; }
        /// <summary>
        /// Subscribe to keyspace notification (if enabled on the redis DB)
        /// </summary>
        public bool EnableKeySpaceNotifications { get; set; }
        /// <summary>
        /// Invalidation callback invoked when InvalidationStrategy=External.
        /// </summary>
        public Action<string> InvalidationCallback { get; set; }

        public InvalidationSettings()
        {
            this.InvalidationStrategy = InvalidationStrategyType.All;
            this.TargetCache = MemoryCache.Default;
            this.EnableKeySpaceNotifications = false;
            this.InvalidationCallback = null;
        }
    }
}
