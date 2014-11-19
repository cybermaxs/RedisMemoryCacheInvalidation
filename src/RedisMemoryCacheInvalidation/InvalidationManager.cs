using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Monitor;
using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace RedisMemoryCacheInvalidation
{
    public static class InvalidationManager
    {
        internal static IRedisNotificationBus notificationBus = null;

        public static bool IsConnected
        {
            get {return notificationBus!=null && notificationBus.Connection.IsConnected;}
        }

        #region Setup
        /// <summary>
        /// Use to Redis MemoryCache Invalidation.
        /// </summary>
        /// <param name="redisConfig">StackExchange configuration settings.</param>
        /// <param name="policy">Cache Invalidation strategy.</param>
        /// <returns>Task when connection is open</returns>
        public static Task Configure(string redisConfig, InvalidationStrategy policy = InvalidationStrategy.ChangeMonitorOnly, MemoryCache cache=null, bool enableKeySpaceNotifications=false)
        {
            if (notificationBus != null)
                throw new InvalidOperationException("Configure() was already called");

            notificationBus = new RedisNotificationBus(redisConfig, policy, cache, enableKeySpaceNotifications);
            return notificationBus.Start();
        }
        #endregion

        #region CreateMonitor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="invalidationKey">invalidation key send by redis {invalidate:key}</param>
        /// <returns></returns>
        public static RedisChangeMonitor CreateChangeMonitor(string invalidationKey)
        {
            if (invalidationKey == null)
                throw new ArgumentNullException("key");

            if (notificationBus == null)
                throw new InvalidOperationException("Configure() was not called");

            if (notificationBus.InvalidationStrategy == InvalidationStrategy.AutoCacheRemoval)
                throw new InvalidOperationException("Could not create a change monitor when policy is DefaultMemoryCacheRemoval");

            return new RedisChangeMonitor(notificationBus.Notifier, invalidationKey);
        }

        /// <summary>
        /// Invalidation as as cache item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
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
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Task with the number of subscribers</returns>
        public static Task<long> Invalidate(string key)
        {
            if (notificationBus == null)
                throw new InvalidOperationException("Configure() was not called");

            return notificationBus.Notify(key);
        }
    }
}
