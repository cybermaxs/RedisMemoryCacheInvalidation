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
        public  InvalidationStrategyType InvalidationStrategy { get; set; }
        public bool EnableKeySpaceNotifications { get; private set; }

        public INotificationManager<string> Notifier { get; set; }
        public IRedisConnection Connection { get; internal set; }
        public MemoryCache LocalCache { get; private set; }
        public Action<string> NotificationCallback { get; private set; }

        private RedisNotificationBus(InvalidationSettings settings)
        {
            this.InvalidationStrategy = settings.InvalidationStrategy;
            this.EnableKeySpaceNotifications = settings.EnableKeySpaceNotifications;
            this.LocalCache = settings.TargetCache;
            this.NotificationCallback = settings.InvalidationCallback;

            this.Notifier = new NotificationManager();
        }

        public RedisNotificationBus(string redisConfiguration, InvalidationSettings settings)
            : this(settings)
        {
            var config = ConfigurationOptions.Parse(redisConfiguration, true);
            this.Connection = RedisConnectionFactory.New(config);
        }

        public RedisNotificationBus(ConnectionMultiplexer mux, InvalidationSettings settings)
            : this(settings)
        {
            this.Connection = RedisConnectionFactory.New(mux);
        }

        public void Start()
        {
            this.Connection.Connect();
            Connection.Subscribe(Constants.DEFAULT_INVALIDATION_CHANNEL, this.OnInvalidationMessage);
            if (this.EnableKeySpaceNotifications)
                Connection.Subscribe(Constants.DEFAULT_KEYSPACE_CHANNEL, this.OnKeySpaceNotificationMessage);
        }

        public void Stop()
        {
            this.Connection.Disconnect();
        }

        public Task<long> NotifyAsync(string key)
        {
            return this.Connection.PublishAsync(Constants.DEFAULT_INVALIDATION_CHANNEL, key);
        }

        #region Notification Handlers
        private void OnInvalidationMessage(RedisChannel pattern, RedisValue data)
        {
            if (pattern == Constants.DEFAULT_INVALIDATION_CHANNEL)
            {
                this.ProcessInvalidationMessage(data);
            }
        }

        private void OnKeySpaceNotificationMessage(RedisChannel pattern, RedisValue data)
        {
            var prefix = pattern.ToString().Substring(0, 10);
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
            this.Stop();
        }
    }
}
