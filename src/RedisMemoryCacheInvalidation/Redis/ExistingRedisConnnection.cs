using RedisMemoryCacheInvalidation.Utils;
using StackExchange.Redis;

namespace RedisMemoryCacheInvalidation.Redis
{
    internal class ExistingRedisConnnection : RedisConnectionBase
    {
        public ExistingRedisConnnection(IConnectionMultiplexer mux)
        {
            Guard.NotNull(mux, nameof(mux));
            multiplexer = mux;
        }

        public override bool Connect()
        {
            return this.multiplexer.IsConnected;
        }

        public override void Disconnect()
        {
            if (this.IsConnected)
            {
                this.multiplexer.GetSubscriber().UnsubscribeAll();
            }
        }
    }
}
