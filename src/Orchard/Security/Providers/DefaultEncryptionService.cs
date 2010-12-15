using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Orchard.Environment.Configuration;
using Orchard.Utility.Extensions;

namespace Orchard.Security.Providers {
    public class DefaultEncryptionService : IEncryptionService {
        private readonly ShellSettings _shellSettings;
        private const int SaltSize = 16;

        public DefaultEncryptionService(ShellSettings shellSettings ) {
            _shellSettings = shellSettings;
        }

        public byte[] Decode(byte[] encodedData) {
            using ( var ms = new MemoryStream() ) {
                using (var algorithm = CreateAlgorithm()) {
                    using ( var cs = new CryptoStream(ms, algorithm.CreateDecryptor(), CryptoStreamMode.Write) ) {
                        cs.Write(encodedData, 0, encodedData.Length);
                        cs.FlushFinalBlock();
                    }

                    // remove the salt part
                    return ms.ToArray().Skip(SaltSize).ToArray();
                }
            }
        }

        public byte[] Encode(byte[] data) { 
            var salt = new byte[SaltSize];

            // generate a random salt to happend to encoded data
            using ( var random = new RNGCryptoServiceProvider() ) {
                random.GetBytes(salt);
            }
            using ( var ms = new MemoryStream() ) {
                using (var algorithm = CreateAlgorithm()) {
                    using ( var cs = new CryptoStream(ms, algorithm.CreateEncryptor(), CryptoStreamMode.Write) ) {
                        // append the salt to the data and encrypt
                        var salted = salt.Concat(data).ToArray();
                        cs.Write(salted, 0, salted.Length);
                        cs.FlushFinalBlock();
                    }

                    return ms.ToArray();
                }
            }
        }

        private SymmetricAlgorithm CreateAlgorithm() {
            var encryptionAlgorithm = SymmetricAlgorithm.Create(_shellSettings.EncryptionAlgorithm);

            encryptionAlgorithm.Key = _shellSettings.EncryptionKey.ToByteArray();
            encryptionAlgorithm.IV = _shellSettings.EncryptionIV.ToByteArray();

            return encryptionAlgorithm;

        }
    }
}
