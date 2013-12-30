namespace RedisMemoryCacheInvalidation
{
    /// <summary>
    /// Redis connection info settings.
    /// </summary>
    public class RedisConnectionInfo
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public int IOTimeout { get; set; }
        public string Password { get; set; }
        public int MaxUnsent { get; set; }
        public bool AllowAdmin { get; set; }
        public int SyncTimeout { get; set; }

        public RedisConnectionInfo()
        {
            //default values
            this.Host="localhost";
            this.Port = 6379;
            this.IOTimeout = -1;
            this.Password = null;
            this.MaxUnsent = 2147483647;
            this.AllowAdmin = false;
            this.SyncTimeout = 10000;
        }
    }
}
