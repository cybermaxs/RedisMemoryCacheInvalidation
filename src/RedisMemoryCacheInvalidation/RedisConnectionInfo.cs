namespace RedisMemoryCacheInvalidation
{
    /// <summary>
    /// Redis connection info settings.
    /// </summary>
    public class RedisConnectionInfo
    {
        public string Host { get; private set; }
        public int Port { get; private set; }
        public int IOTimeout { get; private set; }
        public string Password { get; private set; }
        public int MaxUnsent { get; private set; }
        public bool AllowAdmin { get; private set; }
        public int SyncTimeout { get; private set; }

        public RedisConnectionInfo(string host = "localhost", int port = 6379, int ioTimeout = -1, string password = "", int maxUnsent = int.MaxValue, bool allowAdmin = false, int syncTimeout = 10000)
        {
            this.Host = host;
            this.Port = port;
            this.IOTimeout = ioTimeout;
            this.Password = password;
            this.MaxUnsent = maxUnsent;
            this.AllowAdmin = allowAdmin;
            this.SyncTimeout = syncTimeout;
        }
    }
}
