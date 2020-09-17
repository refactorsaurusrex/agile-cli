using System;

namespace AgileCli.Services
{
    public class AgileCliConfigurationException : Exception
    {
        public AgileCliConfigurationException(string configurationFilePath) => ConfigurationFilePath = configurationFilePath;

        public AgileCliConfigurationException(string message, string configurationFilePath) : base(message) => ConfigurationFilePath = configurationFilePath;

        public AgileCliConfigurationException(string message, Exception innerException, string configurationFilePath) : base(message, innerException) => ConfigurationFilePath = configurationFilePath;

        public string ConfigurationFilePath { get; }
    }
}