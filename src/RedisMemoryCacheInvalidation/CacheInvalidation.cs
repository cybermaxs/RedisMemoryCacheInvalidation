using System;
using RedisMemoryCacheInvalidation.Configuration;

namespace RedisMemoryCacheInvalidation
{
    public static class CacheInvalidation
    {
        public static RedisInvalidationMessageBus RedisBus = null;

        /// <summary>
        /// Use Redis MemoryCacheInvalidation.
        /// Load Redis settings from config file.
        /// </summary>
        public static void UseRedis()
        {
            if (RedisBus != null)
                throw new InvalidOperationException("Redis Connection is already set");

            ConfigurationProvider<RedisConfigurationSection> provider = new ConfigurationProvider<RedisConfigurationSection>(RedisConfigurationSection.SECTION_NAME);
            var redisconfig = provider.Read();

            RedisConnectionInfo connectionInfo = new RedisConnectionInfo()
            {
                Host = redisconfig.Host,
                AllowAdmin = redisconfig.AllowAdmin,
                IOTimeout = redisconfig.IOTimeout,
                Password = redisconfig.Password,
                Port = redisconfig.Port,
                MaxUnsent = redisconfig.MaxUnsent,
                SyncTimeout = redisconfig.SyncTimeout
            };

            RedisBus = new RedisInvalidationMessageBus(connectionInfo);
        }

        /// <summary>
        /// Use Redis MemoryCache Invalidation.
        /// Programmatic configuration.
        /// </summary>
        public static void UseRedis(RedisConnectionInfo connectionInfo)
        {
            if (RedisBus != null)
                throw new InvalidOperationException("Redis Connection is already set");

            RedisBus = new RedisInvalidationMessageBus(connectionInfo);
        }

        public static RedisChangeMonitor CreateChangeMonitor(string key)
        {
            if (RedisBus == null)
                throw new InvalidOperationException("Redis Connection is not set");

            return new RedisChangeMonitor(RedisBus, key);
        }
    }
}
