using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AgileCli.Services
{
    internal class WindowsTokenManager : TokenManagerBase
    {
        private readonly string _cipherPath;
        private readonly string _entropyPath;

        public WindowsTokenManager(string key, string dataDirectoryOverride = "") : base(key, dataDirectoryOverride)
        {
            _cipherPath = Path.Combine(AppDataDirectory, key);
            _entropyPath = Path.Combine(AppDataDirectory, $"{key}bdb132b118af");
        }

        public override void Store(string token)
        {
            var tokenBytes = Encoding.UTF8.GetBytes(token);

            var entropy = new byte[255];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(entropy);

            var cipher = ProtectedData.Protect(tokenBytes, entropy, DataProtectionScope.LocalMachine);

            File.WriteAllBytes(_cipherPath, cipher);
            File.WriteAllBytes(_entropyPath, entropy);
        }

        public override string Retrieve()
        {
            if (!File.Exists(_cipherPath) || !File.Exists(_entropyPath))
                return string.Empty;

            var cipher = File.ReadAllBytes(_cipherPath);
            var entropy = File.ReadAllBytes(_entropyPath);

            var resultBytes = ProtectedData.Unprotect(cipher, entropy, DataProtectionScope.LocalMachine);
            var token = Encoding.UTF8.GetString(resultBytes);

            return token;
        }
    }
}