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
    internal class RedisNotificationBus : IRedisNotificationBus
    {
        private readonly InvalidationSettings _settings;
        public INotificationManager<string> Notifier { get; private set; }
        public IRedisConnection Connection { get; internal set; }

        public InvalidationStrategyType InvalidationStrategy => _settings.InvalidationStrategy;
        public bool EnableKeySpaceNotifications => _settings.EnableKeySpaceNotifications;
        public MemoryCache LocalCache => _settings.TargetCache;
        public Action<string> NotificationCallback => _settings.InvalidationCallback;

        private RedisNotificationBus(InvalidationSettings settings)
        {
            _settings = settings;

            Notifier = new NotificationManager();
        }

        public RedisNotificationBus(string redisConfiguration, InvalidationSettings settings)
            : this(settings)
        {
            Connection = RedisConnectionFactory.New(redisConfiguration);
        }

        public RedisNotificationBus(ConnectionMultiplexer mux, InvalidationSettings settings)
            : this(settings)
        {
            Connection = RedisConnectionFactory.New(mux);
        }

        public void Start()
        {
            Connection.Connect();
            Connection.Subscribe(Constants.DEFAULT_INVALIDATION_CHANNEL, OnInvalidationMessage);
            if (EnableKeySpaceNotifications)
                Connection.Subscribe(Constants.DEFAULT_KEYSPACE_CHANNEL, OnKeySpaceNotificationMessage);
        }

        public void Stop()
        {
            Connection.Disconnect();
        }

        public Task<long> NotifyAsync(string key)
        {
            return Connection.PublishAsync(Constants.DEFAULT_INVALIDATION_CHANNEL, key);
        }

        private void OnInvalidationMessage(RedisChannel pattern, RedisValue data)
        {
            if (pattern == Constants.DEFAULT_INVALIDATION_CHANNEL)
            {
                ProcessInvalidationMessage(data.ToString());
            }
        }

        private void OnKeySpaceNotificationMessage(RedisChannel pattern, RedisValue data)
        {
            var prefix = pattern.ToString().Substring(0, 10);
            switch (prefix)
            {
                case "__keyevent":
                    ProcessInvalidationMessage(data.ToString());
                    break;
                default:
                    //nop
                    break;
            }
        }

        private void ProcessInvalidationMessage(string key)
        {
            if ((InvalidationStrategy & InvalidationStrategyType.ChangeMonitor) == InvalidationStrategyType.ChangeMonitor)
                Notifier.Notify(key);

            if ((InvalidationStrategy & InvalidationStrategyType.AutoCacheRemoval) == InvalidationStrategyType.AutoCacheRemoval)
                if (LocalCache != null)
                    LocalCache.Remove(key);

            if ((InvalidationStrategy & InvalidationStrategyType.External) == InvalidationStrategyType.External)
                if (NotificationCallback != null)
                    NotificationCallback?.Invoke(key);

        }
    }
}
