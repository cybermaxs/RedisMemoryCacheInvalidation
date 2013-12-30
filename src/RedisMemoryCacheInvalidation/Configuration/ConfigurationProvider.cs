using System.Configuration;

namespace RedisMemoryCacheInvalidation.Configuration
{
    public sealed class ConfigurationProvider<TSection> where TSection : class
    {
        /// <summary>
        /// Name of the section to read from the app.config.
        /// </summary>
        private string sectionName;

        /// <summary>
        /// Config object used to read a custom configuration file.
        /// If not set the default file will be used.
        /// </summary>
        private System.Configuration.Configuration config;

        /// <summary>
        /// Initialize the provider.
        /// </summary>
        /// <param name="sectionName"></param>
        public ConfigurationProvider(string sectionName)
        {
            this.sectionName = sectionName;
        }

        /// <summary>
        /// Set a custom configuration file.
        /// </summary>
        /// <param name="config"></param>
        public void SetConfigurationFile(string file)
        {
            // Create the mapping.
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = file;

            // Open the configuration.
            config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
        }

        /// <summary>
        /// Read the configuration file.
        /// </summary>
        /// <returns></returns>
        public TSection Read()
        {
            // Try to read the config section.
            TSection section = GetSection() as TSection;
            if (section == null)
                throw new ConfigurationErrorsException("Error when loading section " + sectionName);

            // Done.
            return section;
        }

        /// <summary>
        /// Get the section from the default configuration file or from the custom one.
        /// </summary>
        /// <returns></returns>
        private object GetSection()
        {
            if (config != null)
                return config.GetSection(sectionName);
            else
                return ConfigurationManager.GetSection(sectionName);
        }
    }
}
