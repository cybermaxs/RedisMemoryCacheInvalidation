using System.Diagnostics;
using System.Linq;
using StackExchange.Redis;

namespace RedisMemoryCacheInvalidation.Tests.Helper
{
    /// <summary>
    /// Helper class to manage a local redis server for integration tests.
    /// </summary>
    public class RedisServer
    {
        public static bool IsRunning
        {
            get
            {
                return Process.GetProcessesByName("redis-server").Count() > 0;
            }
        }

        public static bool Start()
        {
            if (!IsRunning)
            {
                var p = Process.Start(@"..\..\..\..\packages\Redis-64.2.8.17\redis-server.exe");

                using (var cnx = ConnectionMultiplexer.Connect("localhost:6379,allowAdmin=true"))
                {
                    cnx.GetServer("localhost:6379").FlushAllDatabases();
                    cnx.GetServer("localhost:6379").ConfigSet("notify-keyspace-events", "EA");
                }
                return p!= null;//bouh
            }
            return false;
        }

        public static bool Reset()
        {
            if (IsRunning)
            {
                using (var cnx = ConnectionMultiplexer.Connect("localhost:6379,allowAdmin=true"))
                {
                    cnx.GetServer("localhost:6379").FlushAllDatabases();
                }
            }
            return false;
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
