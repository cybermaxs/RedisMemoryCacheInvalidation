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
        private readonly InvalidationSettings settings;
        public INotificationManager<string> Notifier { get; private set; }
        public IRedisConnection Connection { get; internal set; }

        #region Props
        public InvalidationStrategyType InvalidationStrategy { get { return settings.InvalidationStrategy; } }
        public bool EnableKeySpaceNotifications { get { return settings.EnableKeySpaceNotifications; } }
        public MemoryCache LocalCache { get { return settings.TargetCache; } }
        public Action<string> NotificationCallback { get { return settings.InvalidationCallback; } }
        #endregion

        #region Constructors
        private RedisNotificationBus(InvalidationSettings settings)
        {
            this.settings = settings;

            this.Notifier = new NotificationManager();
        }

        public RedisNotificationBus(string redisConfiguration, InvalidationSettings settings)
            : this(settings)
        {
            this.Connection = RedisConnectionFactory.New(redisConfiguration);
        }

        public RedisNotificationBus(ConnectionMultiplexer mux, InvalidationSettings settings)
            : this(settings)
        {
            this.Connection = RedisConnectionFactory.New(mux);
        }
        #endregion

        public void Start()
        {
            this.Connection.Connect();
            Connection.Subscribe(Constants.DEFAULT_INVALIDATION_CHANNEL, OnInvalidationMessage);
            if (this.EnableKeySpaceNotifications)
                Connection.Subscribe(Constants.DEFAULT_KEYSPACE_CHANNEL, OnKeySpaceNotificationMessage);
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
                this.ProcessInvalidationMessage(data.ToString());
            }
        }

        private void OnKeySpaceNotificationMessage(RedisChannel pattern, RedisValue data)
        {
            var prefix = pattern.ToString().Substring(0, 10);
            switch (prefix)
            {
                case "__keyevent":
                    this.ProcessInvalidationMessage(data.ToString());
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
                    NotificationCallback?.Invoke(key);

        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}
