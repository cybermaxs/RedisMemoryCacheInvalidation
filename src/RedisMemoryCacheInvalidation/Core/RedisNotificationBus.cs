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
        public InvalidationStrategyType InvalidationStrategy { get; set; }
        public bool EnableKeySpaceNotifications { get; private set; }

        public INotificationManager<string> Notifier { get; set; }
        public IRedisConnection Connection { get; internal set; }
        public MemoryCache LocalCache { get; private set; }
        public Action<string> NotificationCallback { get; private set; }

        private RedisNotificationBus(InvalidationSettings settings)
        {
            settings = settings ?? new InvalidationSettings();

            this.InvalidationStrategy = settings.InvalidationStrategy;
            this.EnableKeySpaceNotifications = settings.EnableKeySpaceNotifications;
            this.LocalCache = settings.TargetCache;
            this.NotificationCallback = settings.InvalidationCallback;

            this.Notifier = new NotificationManager();
        }

        public RedisNotificationBus(string redisConfiguration, InvalidationSettings settings=null)
            : this(settings)
        {
            var config = ConfigurationOptions.Parse(redisConfiguration, true);
            this.Connection = new StackExchangeRedisConnection(config);
        }

        public RedisNotificationBus(ConnectionMultiplexer mux, InvalidationSettings settings = null)
            : this(settings)
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
            if ((this.InvalidationStrategy & InvalidationStrategyType.ChangeMonitor) == InvalidationStrategyType.ChangeMonitor)
                Notifier.Notify(key);

            if ((this.InvalidationStrategy & InvalidationStrategyType.AutoCacheRemoval) == InvalidationStrategyType.AutoCacheRemoval)
                if (this.LocalCache != null)
                    this.LocalCache.Remove(key);

            if ((this.InvalidationStrategy & InvalidationStrategyType.External) == InvalidationStrategyType.External)
                if (NotificationCallback != null)
                    NotificationCallback(key);

        }

        public void Dispose()
        {
            this.StopAsync();
        }
    }
}
