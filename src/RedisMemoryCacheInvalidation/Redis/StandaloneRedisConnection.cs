using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RedisMemoryCacheInvalidation.Redis
{
    internal class StandaloneRedisConnection : RedisConnectionBase
    {
        private readonly ConfigurationOptions options;
        public StandaloneRedisConnection(ConfigurationOptions configurationOptions)
        {
            this.options = configurationOptions;
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
            if(this.IsConnected)
            {
                this.multiplexer.GetSubscriber().UnsubscribeAll();
                this.multiplexer.Close(true);
            }
        }
    }
}
