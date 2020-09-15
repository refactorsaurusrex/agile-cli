using System.Runtime.InteropServices;

namespace AgileCli.Services
{
    internal class TokenManagerFactory
    {
        private TokenManagerFactory() { }

        public static ITokenManager Create(string key, string dataDirectoryOverride = "")
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new WindowsTokenManager(key, dataDirectoryOverride);

            return new MacTokenManager(key, dataDirectoryOverride);
        }
    }
}