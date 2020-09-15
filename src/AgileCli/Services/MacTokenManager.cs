using System.IO;
using AuthenticatedEncryption;

namespace AgileCli.Services
{
    internal class MacTokenManager : TokenManagerBase
    {
        private readonly string _cryptKeyPath;
        private readonly string _authKeyPath;
        private readonly string _cipherPath;

        public MacTokenManager(string key, string dataDirectoryOverride = "") : base(key, dataDirectoryOverride)
        {
            _cryptKeyPath = Path.Combine(AppDataDirectory, key);
            _authKeyPath = Path.Combine(AppDataDirectory, $"{key}59ebd19a2c1a");
            _cipherPath = Path.Combine(AppDataDirectory, $"{key}8db75a7937b1");

            if (!File.Exists(_cryptKeyPath) || !File.Exists(_authKeyPath))
            {
                var cryptKey = Encryption.NewKey();
                var authKey = Encryption.NewKey();

                File.WriteAllBytes(_cryptKeyPath, cryptKey);
                File.WriteAllBytes(_authKeyPath, authKey);
            }
        }

        public override void Store(string token)
        {
            var cryptKey = File.ReadAllBytes(_cryptKeyPath);
            var authKey = File.ReadAllBytes(_authKeyPath);
            var cipherText = Encryption.Encrypt(token, cryptKey, authKey);
            File.WriteAllText(_cipherPath, cipherText);
        }

        public override string Retrieve()
        {
            var cryptKey = File.ReadAllBytes(_cryptKeyPath);
            var authKey = File.ReadAllBytes(_authKeyPath);
            var cipherText = File.ReadAllText(_cipherPath);
            return Encryption.Decrypt(cipherText, cryptKey, authKey);
        }
    }
}