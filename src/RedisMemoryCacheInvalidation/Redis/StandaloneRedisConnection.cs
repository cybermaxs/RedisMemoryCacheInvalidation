using StackExchange.Redis;
using System;
using System.Reflection;

namespace RedisMemoryCacheInvalidation.Redis
{
    internal class StandaloneRedisConnection : RedisConnectionBase
    {
        private readonly ConfigurationOptions options;
        public StandaloneRedisConnection(string configurationOptions)
        {
            this.options = ConfigurationOptions.Parse(configurationOptions);
        }

        public override bool Connect()
        {
            if (this.multiplexer == null)
            {
                //myope overrides here
                this.options.ConnectTimeout = 5000;
                this.options.ConnectRetry = 3;
                this.options.DefaultVersion = new Version("2.8.0");
                this.options.KeepAlive = 90;
                this.options.AbortOnConnectFail = false;
                this.options.ClientName = "InvalidationClient_" + System.Environment.MachineName + "_" + Assembly.GetCallingAssembly().GetName().Version;

                this.multiplexer = ConnectionMultiplexer.Connect(options);
            }

            return this.multiplexer.IsConnected;
        }

        public override void Disconnect()
        {
            this.UnsubscribeAll();
            this.multiplexer.Close(false);
        }
    }
}
