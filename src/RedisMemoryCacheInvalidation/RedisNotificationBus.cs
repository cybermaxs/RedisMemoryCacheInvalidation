using BookSleeve;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedisMemoryCacheInvalidation
{
    /// <summary>
    /// Main class to manage redis subscriber connection.
    /// </summary>
    public class RedisNotificationBus : IDisposable, IInvalidationMessageBus, ITopicObservable<string>
    {
        public const string INVALIDATION_KEY = "invalidate";
        private readonly Func<RedisConnection> connectionFactory;

        private RedisConnection connection;
        private RedisSubscriberConnection channel;
        private int state;
        private int retryCount;

        public TimeSpan ReconnectDelay { get; set; }
        public RedisCacheInvalidationPolicy InvalidationPolicy { get; set; }
        public int MaxRetries { get; set; }
        public bool IsConnected { get { return this.connection != null && this.state == State.Connected; } }

        /// <summary>
        /// base constructor
        /// </summary>
        /// <param name="configuration"></param>
        public RedisNotificationBus(RedisConnectionInfo configuration, RedisCacheInvalidationPolicy policy)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            this.connectionFactory = () => new RedisConnection(configuration.Host, configuration.Port, configuration.IOTimeout, configuration.Password, configuration.MaxUnsent, configuration.AllowAdmin, configuration.SyncTimeout);

            this.ReconnectDelay = TimeSpan.FromSeconds(1);
            this.InvalidationPolicy = policy;
            this.MaxRetries = 5;

            ConnectWithRetry();
        }

        #region IDisposable
        public void Dispose()
        {
            if (Topics.Count > 0)
                foreach (var topic in Topics)
                {
                    foreach (var observer in topic.Value.ToList())
                        observer.OnCompleted();
                }
            Topics.Clear();

            var oldState = Interlocked.Exchange(ref this.state, State.Disposing);

            switch (oldState)
            {
                case State.Connected:
                    Shutdown();
                    break;
                case State.Closed:
                case State.Disposing:
                    // No-op
                    break;
                case State.Disposed:
                    Interlocked.Exchange(ref this.state, State.Disposed);
                    break;
                default:
                    break;
            }
        }
        #endregion

        private void Shutdown()
        {
            Trace.TraceInformation("Shutdown()");

            if (this.channel != null && this.channel.State != RedisConnectionBase.ConnectionState.Closed)
            {
                this.channel.Unsubscribe(INVALIDATION_KEY).SafeAwaitable();
                this.channel.Close(true);
            }

            if (this.connection != null && this.connection.State != RedisConnectionBase.ConnectionState.Closed)
            {
                this.connection.Close(true);
            }

            Interlocked.Exchange(ref this.state, State.Disposed);
        }

        #region Connection Handlers
        private void OnConnectionClosed(object sender, EventArgs e)
        {
            Trace.TraceInformation("OnConnectionClosed()");

            AttemptReconnect(new RedisConnectionClosedException());
        }

        private void OnConnectionError(object sender, ErrorEventArgs e)
        {
            Trace.TraceError("OnConnectionError - " + e.Cause + ". " + e.Exception.GetBaseException());

            AttemptReconnect(e.Exception);
        }
        #endregion

        private void AttemptReconnect(Exception exception)
        {
            // Change the state to closed and retry connecting
            var oldState = Interlocked.CompareExchange(ref this.state,
                                                       State.Closed,
                                                       State.Connected);
            if (oldState == State.Connected)
            {
                Trace.TraceInformation("Attempting reconnect...");

                // Retry until the connection reconnects
                ConnectWithRetry();
            }
        }

        private void ConnectWithRetry()
        {
            Task connectTask = Connect();

            connectTask.ContinueWith(task =>
            {
                Interlocked.Increment(ref this.retryCount);
                if (task.IsFaulted)
                {
                    Trace.TraceError("Error connecting to Redis - " + task.Exception.GetBaseException());

                    if (this.state == State.Disposing)
                    {
                        Shutdown();
                        return;
                    }


                    if (this.retryCount > this.MaxRetries)
                    {
                        Trace.TraceError("Max Retries reached");
                        Shutdown();
                        return;
                    }

                    TaskHelper.Delay(ReconnectDelay).Then(() => this.ConnectWithRetry());
                }
                else
                {
                    Trace.TraceInformation("Connected");

                    var oldState = Interlocked.CompareExchange(ref this.state,
                                                               State.Connected,
                                                               State.Closed);
                    if (oldState == State.Disposing)
                    {
                        Shutdown();
                    }
                }
            },
            TaskContinuationOptions.ExecuteSynchronously);
        }

        #region ITopicObservable
        public Task<long> Notify(string key)
        {
            try
            {
                if (this.connection == null)
                    throw new RedisConnectionClosedException();

                return this.connection.Publish(INVALIDATION_KEY, key);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Could not send invalidation Message. Reason : " + ex.Message);
                return TaskHelper.FromResult<long>(0);
            }
        }
        #endregion

        private ConcurrentDictionary<string, List<IObserver<string>>> Topics = new ConcurrentDictionary<string, List<IObserver<string>>>();

        private bool RemoveFromDefaultMemoryCache(string key)
        {
            var cache = MemoryCache.Default;
            return cache.Remove(key) != null;
        }
        public IDisposable Subscribe(string topic, IObserver<string> observer)
        {
            var subObs = Topics.GetOrAdd(topic, new List<IObserver<string>>());

            if (!subObs.Contains(observer))
                subObs.Add(observer);

            return new Unsubscriber(subObs, observer);
        }

        public void Invalidate(string key)
        {
            var observers = Topics.GetOrAdd(key, new List<IObserver<string>>());

            foreach (IObserver<string> observer in observers.ToList())
            {
                observer.OnNext(key);
            }
        }


        private void OnMessage(string pattern, byte[] data)
        {
            // note : we have set the mode to preserve order on the subscription)
            // The key is the stream id (channel)
            var key = Encoding.Default.GetString(data);

            switch (this.InvalidationPolicy)
            {
                case RedisCacheInvalidationPolicy.ChangeMonitorOnly:
                    Invalidate(key);
                    break;
                case RedisCacheInvalidationPolicy.DefaultMemoryCacheRemoval:
                    RemoveFromDefaultMemoryCache(key);
                    break;
                case RedisCacheInvalidationPolicy.Mixed:
                    RemoveFromDefaultMemoryCache(key);
                    Invalidate(key);
                    break;
            }
        }

        private Task Connect()
        {
            if (this.connection != null)
            {
                this.connection.Closed -= OnConnectionClosed;
                this.connection.Error -= OnConnectionError;
                this.connection.Dispose();
                this.connection = null;
            }

            // Create a new connection to redis with the factory
            RedisConnection connection = connectionFactory();

            connection.Closed += OnConnectionClosed;
            connection.Error += OnConnectionError;

            try
            {
                Trace.TraceInformation("Connecting...");

                // Start the connection
                return connection.Open().Then(() =>
                {
                    Trace.TraceInformation("Connection opened");

                    // Create a subscription channel in redis
                    RedisSubscriberConnection channel = connection.GetOpenSubscriberChannel();
                    channel.CompletionMode = ResultCompletionMode.PreserveOrder;

                    // Subscribe to the registered connections
                    return channel.Subscribe(INVALIDATION_KEY, OnMessage).Then(() =>
                    {
                        Trace.TraceInformation("Subscribed to event " + INVALIDATION_KEY);

                        this.channel = channel;
                        this.connection = connection;
                    });
                });

            }
            catch (Exception ex)
            {
                Trace.TraceError("Error connecting to Redis - " + ex.GetBaseException());

                return TaskHelper.FromResult(ex);
            }
        }

        private static class State
        {
            public const int Closed = 0;
            public const int Connected = 1;
            public const int Disposing = 2;
            public const int Disposed = 3;
        }
    }
}
