using System.Diagnostics;
using System.Linq;
using BookSleeve;

namespace RedisMemoryCacheInvalidation.Tests.Helper
{
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
                return Process.Start(@"..\..\..\..\tools\redis-server.exe") != null;
            return false;
        }

        public static bool Reset()
        {
            if (IsRunning)
            {
                using (var cnx = new RedisConnection("localhost", allowAdmin: true))
                {
                    cnx.Wait(cnx.Open());
                    cnx.Server.FlushAll().Wait();
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
