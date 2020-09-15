using System;
using System.IO;

namespace AgileCli.Services
{
    public abstract class TokenManagerBase : ITokenManager
    {
        protected TokenManagerBase(string key, string dataDirectoryOverride = "")
        {
            Key = key;
            var appData = string.IsNullOrWhiteSpace(dataDirectoryOverride)
                ? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                : dataDirectoryOverride;
            AppDataDirectory = Path.Combine(appData, "AgileCli");

            if (!Directory.Exists(AppDataDirectory))
                Directory.CreateDirectory(AppDataDirectory);
        }

        protected string Key { get; }

        public string AppDataDirectory { get; }

        public abstract void Store(string token);

        public abstract string Retrieve();
    }
}