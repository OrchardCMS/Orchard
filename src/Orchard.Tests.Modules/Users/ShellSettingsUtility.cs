using System.Security.Cryptography;
using Orchard.Environment.Configuration;
using Orchard.Utility.Extensions;

namespace Orchard.Tests.Modules.Users {
    public class ShellSettingsUtility {
        public static ShellSettings CreateEncryptionEnabled() {
            // generate random keys for encryption
            var key = new byte[32];
            var iv = new byte[16];
            using ( var random = new RNGCryptoServiceProvider() ) {
                random.GetBytes(key);
                random.GetBytes(iv);
            }

            return new ShellSettings {
                Name = "Alpha",
                RequestUrlHost = "wiki.example.com",
                RequestUrlPrefix = "~/foo",
                EncryptionAlgorithm = "AES",
                EncryptionKey = key.ToHexString(),
                EncryptionIV = iv.ToHexString()
            };
        }
    }
}
