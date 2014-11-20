using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Monitor;
using StackExchange.Redis;
using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace RedisMemoryCacheInvalidation
{
    /// <summary>
    /// Libray's entry point. 
    /// </summary>
    public static class InvalidationManager
    {
        internal static IRedisNotificationBus notificationBus = null;

        /// <summary>
        /// Redis connection state : connected or not.
        /// </summary>
        public static bool IsConnected
        {
            get {return notificationBus!=null && notificationBus.Connection.IsConnected;}
        }

        #region Setup
        /// <summary>
        /// Use to Configure Redis MemoryCache Invalidation.
        /// A new redis connection will be establish based upon parameter redisConfig.
        /// </summary>
        /// <param name="redisConfig">StackExchange configuration settings.</param>
        /// <param name="policy">Cache Invalidation strategy(</param>
        /// <param name="cache">Target MemoryCache for automatic removal</param>
        /// <param name="enableKeySpaceNotifications">Subcribe to all keyspace notifications. Redis server config should be properly configured.</param>
        /// <returns>Task when connection is opened and subcribed to pubsub events.</returns>
        public static Task ConfigureAsync(string redisConfig, InvalidationStrategy policy = InvalidationStrategy.Both, MemoryCache cache=null, bool enableKeySpaceNotifications=false)
        {
            if (notificationBus != null)
                throw new InvalidOperationException("Configure() was already called");

            notificationBus = new RedisNotificationBus(redisConfig, policy, cache, enableKeySpaceNotifications);
            return notificationBus.StartAsync();
        }

        /// <summary>
        /// Use to Configure Redis MemoryCache Invalidation.
        /// </summary>
        /// <param name="mux">Reusing an existing ConnectionMultiplexer.</param>
        /// <param name="policy">Cache Invalidation strategy(</param>
        /// <param name="cache">Target MemoryCache for automatic removal</param>
        /// <param name="enableKeySpaceNotifications">Subcribe to all keyspace notifications. Redis server config should be properly configured.</param>
        /// <returns>Task when connection is opened and subcribed to pubsub events.</returns>
        public static Task ConfigureAsync(ConnectionMultiplexer mux, InvalidationStrategy policy = InvalidationStrategy.Both, MemoryCache cache = null, bool enableKeySpaceNotifications = false)
        {
            if (notificationBus != null)
                throw new InvalidOperationException("Configure() was already called");

            notificationBus = new RedisNotificationBus(mux, policy, cache, enableKeySpaceNotifications);
            return notificationBus.StartAsync();
        }
        #endregion

        #region CreateMonitor
        /// <summary>
        /// Allow to create a custom ChangeMonitor depending on the pubsub event (channel : invalidate, data:invalidationKey)
        /// </summary>
        /// <param name="invalidationKey">invalidation key send by redis PUBLISH invalidate invalidatekey</param>
        /// <returns>RedisChangeMonitor watching for notifications</returns>
        public static RedisChangeMonitor CreateChangeMonitor(string invalidationKey)
        {
            if (string.IsNullOrEmpty(invalidationKey))
                throw new ArgumentNullException("key");

            if (notificationBus == null)
                throw new InvalidOperationException("Configure() was not called");

            if (notificationBus.InvalidationStrategy == InvalidationStrategy.AutoCacheRemoval)
                throw new InvalidOperationException("Could not create a change monitor when policy is DefaultMemoryCacheRemoval");

            return new RedisChangeMonitor(notificationBus.Notifier, invalidationKey);
        }

        /// <summary>
        /// Allow to create a custom ChangeMonitor depending on the pubsub event (channel : invalidate, data:item.Key)
        /// </summary>
        /// <param name="invalidationKey">invalidation key send by redis PUBLISH invalidate invalidatekey</param>
        /// <returns>RedisChangeMonitor watching for notifications</returns>
        public static RedisChangeMonitor CreateChangeMonitor(CacheItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (notificationBus == null)
                throw new InvalidOperationException("Configure() was not called");

            return new RedisChangeMonitor(notificationBus.Notifier, item.Key);
        }
        #endregion

        /// <summary>
        /// Used to send invalidation message for a key.
        /// Shortcut for PUBLISH invalidate key. 
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Task with the number of subscribers</returns>
        public static Task<long> InvalidateAsync(string key)
        {
            if (notificationBus == null)
                throw new InvalidOperationException("Configure() was not called");

            return notificationBus.NotifyAsync(key);
        }
    }
}
