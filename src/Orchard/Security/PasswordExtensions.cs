using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace Orchard.Security {
    public static class PasswordExtensions {

        public const string PBKDF2 = "PBKDF2";

        public static string ComputeHashBase64(string hashAlgorithmName, byte[] saltBytes, string password) {
            var combinedBytes = CombineSaltAndPassword(saltBytes, password);

            // Extending HashAlgorithm would be too complicated: http://stackoverflow.com/questions/6460711/adding-a-custom-hashalgorithmtype-in-c-sharp-asp-net?lq=1
            if (hashAlgorithmName == PBKDF2) {
                // HashPassword() already returns a base64 string.
                return Crypto.HashPassword(Encoding.Unicode.GetString(combinedBytes));
            }
            else {
                using (var hashAlgorithm = HashAlgorithm.Create(hashAlgorithmName)) {
                    return Convert.ToBase64String(hashAlgorithm.ComputeHash(combinedBytes));
                }
            }
        }

        public static byte[] CombineSaltAndPassword(byte[] saltBytes, string password) {
            var passwordBytes = Encoding.Unicode.GetBytes(password);
            return saltBytes.Concat(passwordBytes).ToArray();
        }

    }
}
