using System.Configuration;

namespace RedisMemoryCacheInvalidation.Configuration
{
    public class RedisConfigurationSection : ConfigurationSection
    {
        public const string SECTION_NAME = "redis";

        [ConfigurationProperty("host", IsRequired = true, IsKey = true)]
        public string Host
        {
            get { return (string)base["host"]; }
            set { base["host"] = value; }
        }

        [ConfigurationProperty("port", IsRequired = false, DefaultValue = 6379)]
        public int Port
        {
            get { return (int)base["port"]; }
            set { base["port"] = value; }
        }

        [ConfigurationProperty("iotimeout", IsRequired = false, DefaultValue = -1)]
        public int IOTimeout
        {
            get { return (int)base["iotimeout"]; }
            set { base["iotimeout"] = value; }
        }

        [ConfigurationProperty("password", IsRequired = false, DefaultValue = null)]
        public string Password
        {
            get { return (string)base["password"]; }
            set { base["password"] = value; }
        }

        [ConfigurationProperty("maxunsent", IsRequired = false, DefaultValue = 2147483647)]
        public int MaxUnsent
        {
            get { return (int)base["maxunsent"]; }
            set { base["maxunsent"] = value; }
        }

        [ConfigurationProperty("allowadmin", IsRequired = false, DefaultValue = false)]
        public bool AllowAdmin
        {
            get { return (bool)base["allowadmin"]; }
            set { base["allowadmin"] = value; }
        }

        [ConfigurationProperty("synctimeout", IsRequired = false, DefaultValue = 10000)]
        public int SyncTimeout
        {
            get { return (int)base["synctimeout"]; }
            set { base["synctimeout"] = value; }
        }

        public RedisConfigurationSection()
        {
        }
    }
}
