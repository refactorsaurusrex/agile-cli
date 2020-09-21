using System;
using System.IO;
using YamlDotNet.Serialization;

namespace AgileCli.Services
{
    internal class AgileCliConfigurationManager : IAgileCliConfiguration
    {
        private readonly string _filePath;
        private int _sprintTargetPercent;
        private int _defaultSprintCount;
        private const int AppDefaultSprintCount = 5;
        private const int DefaultSprintTargetPercent = 15;

        private static string GetFilePath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appDataDirectory = Path.Combine(appData, "AgileCli");

            if (!Directory.Exists(appDataDirectory))
                Directory.CreateDirectory(appDataDirectory);

            return Path.Combine(appDataDirectory, "configuration.yaml");
        }

        public static IAgileCliConfiguration Load()
        {
            var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
            var filePath = GetFilePath();
            if (!File.Exists(filePath)) 
                return new AgileCliConfigurationManager();
            var text = File.ReadAllText(GetFilePath());

            try
            {
                return deserializer.Deserialize<AgileCliConfigurationManager>(text);
            }
            catch (Exception e)
            {
                const string message = "An error occurred while loading your AgileCli configuration file. Please open the file, ensure all "+
                                       "values are valid, and try again. If you need help, refer to the documentation on GitHub.";
                throw new AgileCliConfigurationException(message, e, filePath);
            }
        }

        public AgileCliConfigurationManager() => _filePath = GetFilePath();

        public string JiraHostName { get; set; } = "example.atlassian.net";

        public string DefaultBoardName { get; set; }

        public int DefaultSprintCount
        {
            get => _defaultSprintCount > 50 || _defaultSprintCount < 1 ? AppDefaultSprintCount : _defaultSprintCount;
            set => _defaultSprintCount = value;
        }

        public int SprintTargetPercent
        {
            get => _sprintTargetPercent > 100 || _sprintTargetPercent <= 0 ? DefaultSprintTargetPercent : _sprintTargetPercent;
            set => _sprintTargetPercent = value;
        }

        public double GetSprintTargetPercent() => SprintTargetPercent / 100D;

        public void Save()
        {
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(this);
            File.WriteAllText(_filePath, yaml);
        }

        public void Open(ISystemProcess process)
        {
            Save();
            process.Start(_filePath);
        }
    }
}