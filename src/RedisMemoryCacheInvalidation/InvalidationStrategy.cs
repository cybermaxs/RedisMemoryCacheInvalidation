namespace RedisMemoryCacheInvalidation
{
    /// <summary>
    /// Cache Invalidation Policy.
    /// </summary>
    public enum InvalidationStrategy
    {
        /// <summary>
        /// Use only change monitor to invalidate local items.
        /// </summary>
        ChangeMonitorOnly=0,
        /// <summary>
        /// Auto remove items from default memory cache.
        /// </summary>
        AutoCacheRemoval,
        /// <summary>
        /// Both policies.
        /// </summary>
        Both
    }
}
