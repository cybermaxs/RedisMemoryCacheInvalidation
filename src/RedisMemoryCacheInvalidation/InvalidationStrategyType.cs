using System;
namespace RedisMemoryCacheInvalidation
{
    /// <summary>
    /// Cache Invalidation Strategy.
    /// </summary>
    [Flags]
    public enum InvalidationStrategyType
    {
        Undefined = 0,
        /// <summary>
        /// Use only change monitor to invalidate local items.
        /// </summary>
        ChangeMonitor=1,
        /// <summary>
        /// Auto remove items from default memory cache.
        /// </summary>
        AutoCacheRemoval=2,
        /// <summary>
        /// External. An event is emitted.
        /// </summary>
        External = 4,
        /// <summary>
        /// All.
        /// </summary>
        All = ~0
    }
}
