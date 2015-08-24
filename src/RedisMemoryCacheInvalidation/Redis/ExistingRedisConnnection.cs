using RedisMemoryCacheInvalidation.Utils;
using StackExchange.Redis;

namespace RedisMemoryCacheInvalidation.Redis
{
    internal class ExistingRedisConnnection : RedisConnectionBase
    {
        public ExistingRedisConnnection(IConnectionMultiplexer mux)
        {
            multiplexer = mux;
        }

        public override bool Connect()
        {
            return this.IsConnected;
        }

        public override void Disconnect()
        {
            this.UnsubscribeAll();
        }
    }
}
