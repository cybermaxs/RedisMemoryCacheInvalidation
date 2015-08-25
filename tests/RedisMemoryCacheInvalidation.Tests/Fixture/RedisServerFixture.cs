using System;
using System.Linq;
using System.Diagnostics;
using StackExchange.Redis;
using System.Threading;

namespace RedisMemoryCacheInvalidation.Tests.Fixtures
{
    public class RedisServerFixture : IDisposable
    {
        private Process server;
        private ConnectionMultiplexer mux;

        private bool wasStarted;

        public RedisServerFixture()
        {
            if (!IsRunning)
            {
                this.server = Process.Start(@"..\..\..\..\packages\Redis-64.2.8.21\redis-server.exe");
                wasStarted = true;
            }
            Thread.Sleep(1000);
            this.mux = ConnectionMultiplexer.Connect("localhost:6379,allowAdmin=true");
            this.mux.GetServer("localhost: 6379").ConfigSet("notify-keyspace-events", "KEA");
        }

        public void Dispose()
        {
            if (this.mux != null && this.mux.IsConnected)
                this.mux.Close(false);

            if (server != null && !server.HasExited && wasStarted)
                server.Kill();
        }

        public IDatabase GetDatabase(int db)
        {
            return this.mux.GetDatabase(db);
        }

        public ISubscriber GetSubscriber()
        {
            return this.mux.GetSubscriber();
        }

        public static bool IsRunning
        {
            get
            {
                return Process.GetProcessesByName("redis-server").Count() > 0;
            }
        }

        public void Reset()
        {
            this.mux.GetServer("localhost:6379").FlushAllDatabases();
        }

        public static void Kill()
        {
            foreach (var p in Process.GetProcessesByName(@"redis-server"))
            {
                p.Kill();
            }
        }
    }
}
