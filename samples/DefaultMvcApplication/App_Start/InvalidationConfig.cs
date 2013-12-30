using RedisMemoryCacheInvalidation;

namespace DefaultMvcApplication
{
    public static class InvalidationConfig
    {
        public static void Register()
        {
            // be sure a redis instance is running on localhost
            // check /tools/redis-server.exe

            CacheInvalidation.UseRedis(new RedisConnectionInfo() { Host = "localhost" });
        }
    }
}
