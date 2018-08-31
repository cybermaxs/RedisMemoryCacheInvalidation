using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Monitor;
using RedisMemoryCacheInvalidation.Utils;
using StackExchange.Redis;
using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace RedisMemoryCacheInvalidation
{
    /// <summary>
    /// InvalidationManager
    /// Entry-point to library
    /// </summary>
    public static class InvalidationManager
    {
        internal static IRedisNotificationBus NotificationBus;

        /// <summary>
        /// Redis connection state : connected or not.
        /// </summary>
        public static bool IsConnected => NotificationBus!=null && NotificationBus.Connection.IsConnected;

        /// <summary>
        /// Use to Configure Redis MemoryCache Invalidation.
        /// A new redis connection will be establish based upon parameter redisConfig.
        /// </summary>
        /// <param name="redisConfig">StackExchange configuration settings.</param>
        /// <param name="settings">InvalidationManager settings.(</param>
        /// <returns>Task when connection is opened and subscribed to pubsub events.</returns>
        public static void Configure(string redisConfig, InvalidationSettings settings)
        {
            if (NotificationBus == null)
            {
                NotificationBus = new RedisNotificationBus(redisConfig, settings);
                NotificationBus.Start();
            }
        }

        /// <summary>
        /// Use to Configure Redis MemoryCache Invalidation.
        /// </summary>
        /// <param name="mux">Reusing an existing ConnectionMultiplexer.</param>
        /// <param name="settings">InvalidationManager settings.(</param>
        /// <returns>Task when connection is opened and subscribed to pubsub events.</returns>
        public static void Configure(ConnectionMultiplexer mux, InvalidationSettings settings)
        {
            if (NotificationBus == null)
            {
                NotificationBus = new RedisNotificationBus(mux, settings);
                NotificationBus.Start();
            }
        }

        /// <summary>
        /// Allow to create a custom ChangeMonitor depending on the pubsub event (channel : invalidate, data:invalidationKey)
        /// </summary>
        /// <param name="invalidationKey">invalidation key send by redis PUBLISH invalidate invalidatekey</param>
        /// <returns>RedisChangeMonitor watching for notifications</returns>
        public static RedisChangeMonitor CreateChangeMonitor(string invalidationKey)
        {
            Guard.NotNullOrEmpty(invalidationKey, nameof(invalidationKey));

            EnsureConfiguration();

            if (NotificationBus.InvalidationStrategy == InvalidationStrategyType.AutoCacheRemoval)
                throw new InvalidOperationException("Could not create a change monitor when InvalidationStrategy is DefaultMemoryCacheRemoval");

            return new RedisChangeMonitor(NotificationBus.Notifier, invalidationKey);
        }

        /// <summary>
        /// Allow to create a custom ChangeMonitor depending on the pubsub event (channel : invalidate, data:item.Key)
        /// </summary>
        /// <param name="item">Cache item</param>
        /// <returns>RedisChangeMonitor watching for notifications</returns>
        public static RedisChangeMonitor CreateChangeMonitor(CacheItem item)
        {
            Guard.NotNull(item, nameof(item));

            EnsureConfiguration();

            return new RedisChangeMonitor(NotificationBus.Notifier, item.Key);
        }

        /// <summary>
        /// Used to send invalidation message for a key.
        /// Shortcut for PUBLISH invalidate key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Task with the number of subscribers</returns>
        public static Task<long> InvalidateAsync(string key)
        {
            Guard.NotNullOrEmpty(key, nameof(key));

            EnsureConfiguration();

            return NotificationBus.NotifyAsync(key);
        }

        private static void EnsureConfiguration()
        {
            if (NotificationBus == null)
                throw new InvalidOperationException("Configure() was not called");
        }
    }
}
