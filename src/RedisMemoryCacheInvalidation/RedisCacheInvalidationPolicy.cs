namespace RedisMemoryCacheInvalidation
{
    /// <summary>
    /// Cache Invalidation Policy.
    /// </summary>
    public enum RedisCacheInvalidationPolicy
    {
        /// <summary>
        /// Use only change monitor to invalidate local items.
        /// </summary>
        ChangeMonitorOnly=0,
        /// <summary>
        /// Auto remove items from default memory cache.
        /// </summary>
        DefaultMemoryCacheRemoval,
        /// <summary>
        /// Both policies.
        /// </summary>
        Mixed
    }
}
