using RedisMemoryCacheInvalidation.Utils;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace RedisMemoryCacheInvalidation.Redis
{
    internal class StackExchangeRedisConnection : IRedisConnection, IDisposable
    {
        private ConnectionMultiplexer multiplexer;
        public ConfigurationOptions ConfigurationOptions { get; private set; }

        public StackExchangeRedisConnection(ConfigurationOptions configurationOptions)
        {
            this.ConfigurationOptions = configurationOptions;
        }

        public StackExchangeRedisConnection(string configurationOptions)
        {
            this.ConfigurationOptions = ConfigurationOptions.Parse(configurationOptions);
        }

        public async Task<bool> Connect()
        {
            //myope overrides here
            this.ConfigurationOptions.ConnectTimeout = 5000;
            this.ConfigurationOptions.ConnectRetry = 3;
            this.ConfigurationOptions.DefaultVersion = new Version("2.8.0");
            this.ConfigurationOptions.KeepAlive = 90;
            this.ConfigurationOptions.AbortOnConnectFail = false;
            this.ConfigurationOptions.ClientName = "InvalidationClient_" + System.Environment.MachineName + "_" + Assembly.GetCallingAssembly().GetName().Version;

            this.multiplexer = await ConnectionMultiplexer.ConnectAsync(ConfigurationOptions);

            return this.multiplexer.IsConnected;
        }

        public async Task Disconnect()
        {
            if (this.multiplexer.IsConnected)
            {
                await this.UnsubscribeAllAsync();
                await this.multiplexer.CloseAsync(false);
            }
        }

        #region IRedisConnection
        public bool IsConnected
        {
            get { return this.multiplexer != null && this.multiplexer.IsConnected; }
        }

        public Task SubscribeAsync(string channel, Action<string, string> handler)
        {
            if (this.IsConnected)
            {
                var subscriber = this.multiplexer.GetSubscriber();
                return subscriber.SubscribeAsync(channel, new Action<RedisChannel, RedisValue>((c, v) =>
                    {
                        if (!v.IsNullOrEmpty && !c.IsNullOrEmpty)
                            handler(c, v);
                    }));
            }
            else
                return TaskCache.FromResult<long>(0L);
        }

        public Task UnsubscribeAllAsync()
        {
            if (this.IsConnected)
                return this.multiplexer.GetSubscriber().UnsubscribeAllAsync();
            else
                return TaskCache.Empty;
        }

        public Task<long> PublishAsync(string channel, string value)
        {
            if (this.IsConnected)
            {
                return this.multiplexer.GetSubscriber().PublishAsync(channel, value);
            }
            else
                return TaskCache.FromResult<long>(0L);
        }
        public Task<KeyValuePair<string, string>[]> GetConfigAsync()
        {
            if (this.IsConnected)
            {
                return this.multiplexer.GetServer(this.ConfigurationOptions.EndPoints.First()).ConfigGetAsync();
            }
            else
                return TaskCache.FromResult<KeyValuePair<string, string>[]>(new KeyValuePair<string, string>[] { });
        }
        #endregion

        public void Dispose()
        {
            if (this.multiplexer != null)
                multiplexer.Close(false);
        }
    }
}
