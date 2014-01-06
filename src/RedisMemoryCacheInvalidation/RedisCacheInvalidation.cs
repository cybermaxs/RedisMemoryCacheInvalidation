using RedisMemoryCacheInvalidation.Configuration;
using System;
using System.Runtime.Caching;

namespace RedisMemoryCacheInvalidation
{
    public static class RedisCacheInvalidation
    {
        public static Lazy<RedisNotificationBus> RedisBus = null;

        #region Setup
        /// <summary>
        /// Use Redis MemoryCacheInvalidation.
        /// Load Redis settings from config file.
        /// </summary>
        public static void Use(RedisCacheInvalidationPolicy policy = RedisCacheInvalidationPolicy.ChangeMonitorOnly)
        {
            if (RedisBus != null)
                throw new InvalidOperationException("Redis Connection is already set");

            ConfigurationProvider<RedisConfigurationSection> provider = new ConfigurationProvider<RedisConfigurationSection>(RedisConfigurationSection.SECTION_NAME);
            var redisconfig = provider.Read();

            RedisConnectionInfo connectionInfo = new RedisConnectionInfo(

                host: redisconfig.Host,
                allowAdmin: redisconfig.AllowAdmin,
                ioTimeout: redisconfig.IOTimeout,
                password: redisconfig.Password,
                port: redisconfig.Port,
                maxUnsent: redisconfig.MaxUnsent,
                syncTimeout: redisconfig.SyncTimeout
            );

            RedisBus = new Lazy<RedisNotificationBus>(() => { return new RedisNotificationBus(connectionInfo, policy); });
        }

        /// <summary>
        /// Use Redis MemoryCache Invalidation.
        /// Programmatic configuration.
        /// </summary>
        public static void Use(RedisConnectionInfo connectionInfo, RedisCacheInvalidationPolicy policy = RedisCacheInvalidationPolicy.ChangeMonitorOnly)
        {
            if (RedisBus != null)
                throw new InvalidOperationException("Redis Connection is already set");

            RedisBus = new Lazy<RedisNotificationBus>(() => { return new RedisNotificationBus(connectionInfo, policy); });
        }
        #endregion

        #region CreateMonitor
        public static RedisChangeMonitor CreateChangeMonitor(string key)
        {
            if (RedisBus == null)
                throw new InvalidOperationException("Redis Connection was not set.");

            if (RedisBus.Value.InvalidationPolicy == RedisCacheInvalidationPolicy.DefaultMemoryCacheRemoval)
                throw new InvalidOperationException("Could not create a change monitor when policy is DefaultMemoryCacheRemoval");

            return new RedisChangeMonitor(RedisBus.Value, key);
        }

        public static RedisChangeMonitor CreateChangeMonitor(CacheItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (RedisBus == null)
                throw new InvalidOperationException("Redis Connection was not set");

            return new RedisChangeMonitor(RedisBus.Value, item.Key);
        }
        #endregion
    }
}
