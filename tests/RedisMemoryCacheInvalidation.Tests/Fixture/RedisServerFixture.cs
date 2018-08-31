using System;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisMemoryCacheInvalidation.Tests.Fixtures
{
    public class RedisServerFixture : IDisposable
    {
        private static RedisInside.Redis redis;
        private static string RedisEndpoint;
        private readonly ConnectionMultiplexer mux;

        public RedisServerFixture()
        {
            redis = new RedisInside.Redis();
            Thread.Sleep(100);
            mux = ConnectionMultiplexer.Connect(new ConfigurationOptions { AllowAdmin = true, AbortOnConnectFail = false, EndPoints = { redis.Endpoint } });
            RedisEndpoint = redis.Endpoint.ToString();
            mux.GetServer(redis.Endpoint.ToString()).ConfigSet("notify-keyspace-events", "KEA");

            mux.GetDatabase().StringSetAsync("key", "value");
            var actualValue = mux.GetDatabase().StringGetAsync("key"); ;
        }

        public static bool IsRunning => redis != null;

        public void Dispose()
        {
            if (mux != null && mux.IsConnected)
                mux.Close(false);
            redis.Dispose();
        }

        public IDatabase GetDatabase(int db)
        {
            return mux.GetDatabase(db);
        }
        public string GetEndpoint()
        {
            return RedisEndpoint;
        }

        public ISubscriber GetSubscriber()
        {
            return mux.GetSubscriber();
        }

        public void Reset()
        {
            mux.GetServer(redis.Endpoint.ToString()).FlushAllDatabases();
        }
    }
}