using RedisMemoryCacheInvalidation.Redis;
using StackExchange.Redis;
using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace RedisMemoryCacheInvalidation.Core
{
    /// <summary>
    /// Invalidation message bus. 
    /// </summary>
    internal class RedisNotificationBus : IDisposable, IRedisNotificationBus
    {
        public const string DEFAULT_INVALIDATION_CHANNEL = "invalidate";
        public const string DEFAULT_KEYSPACE_CHANNEL = "__keyevent*__:*";
        public InvalidationStrategy InvalidationStrategy { get; set; }
        public bool EnableKeySpaceNotifications { get; private set; }

        public INotificationManager<string> Notifier { get; set; }
        public IRedisConnection Connection { get; internal set; }
        public MemoryCache LocalCache { get; private set; }

        private RedisNotificationBus(InvalidationStrategy policy = InvalidationStrategy.Both, MemoryCache cache = null, bool enableKeySpaceNotifications = false)
        {
            this.InvalidationStrategy = policy;
            this.EnableKeySpaceNotifications = enableKeySpaceNotifications;

            this.LocalCache = cache ?? MemoryCache.Default;
            this.Notifier = new NotificationManager();
        }

        public RedisNotificationBus(string redisConfiguration, InvalidationStrategy policy = InvalidationStrategy.Both, MemoryCache cache = null, bool enableKeySpaceNotifications = false)
            : this(policy, cache, enableKeySpaceNotifications)
        {
            var config = ConfigurationOptions.Parse(redisConfiguration, true);
            this.Connection = new StackExchangeRedisConnection(config);
        }

        public RedisNotificationBus(ConnectionMultiplexer mux, InvalidationStrategy policy = InvalidationStrategy.Both, MemoryCache cache = null, bool enableKeySpaceNotifications = false)
            : this(policy, cache, enableKeySpaceNotifications)
        {
            this.Connection = new StackExchangeRedisConnection(mux);
        }

        public async Task StartAsync()
        {
            await this.Connection.ConnectAsync().ConfigureAwait(false);
            await Connection.SubscribeAsync(DEFAULT_INVALIDATION_CHANNEL, this.OnInvalidationMessage).ConfigureAwait(false);
            if (this.EnableKeySpaceNotifications)
                await Connection.SubscribeAsync(DEFAULT_KEYSPACE_CHANNEL, this.OnKeySpaceNotificationMessage).ConfigureAwait(false);
        }

        public Task StopAsync()
        {
            return this.Connection.DisconnectAsync();
        }

        public Task<long> NotifyAsync(string key)
        {
            return this.Connection.PublishAsync(DEFAULT_INVALIDATION_CHANNEL, key);
        }

        #region Notification Handlers
        private void OnInvalidationMessage(string pattern, string data)
        {
            if (pattern == DEFAULT_INVALIDATION_CHANNEL)
            {
                this.ProcessInvalidationMessage(data);
            }
        }

        private void OnKeySpaceNotificationMessage(string pattern, string data)
        {
            string prefix = pattern.Substring(0, 10);
            switch (prefix)
            {
                //case "__keyspace":
                //    var parts = pattern.Split(':');
                //    if (parts.Length == 2)
                //        this.ProcessInvalidationMessage(parts[1]);
                //    break;
                case "__keyevent":
                    this.ProcessInvalidationMessage(data);
                    break;
                default:
                    //nop
                    break;
            }
        }
        #endregion

        private void ProcessInvalidationMessage(string key)
        {
            switch (this.InvalidationStrategy)
            {
                case InvalidationStrategy.ChangeMonitorOnly:
                    //call all monitors
                    Notifier.Notify(key);
                    break;
                case InvalidationStrategy.AutoCacheRemoval:
                    //call default cache
                    if (this.LocalCache != null)
                        this.LocalCache.Remove(key);
                    break;
                case InvalidationStrategy.Both:
                    //call both
                    Notifier.Notify(key);
                    if (this.LocalCache != null)
                        this.LocalCache.Remove(key);
                    break;
            }
        }

        public void Dispose()
        {
            this.StopAsync();
        }
    }
}
