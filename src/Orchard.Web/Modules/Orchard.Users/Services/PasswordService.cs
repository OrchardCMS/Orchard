using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Security;
using Orchard.Environment.Configuration;
using Orchard.Security;

namespace Orchard.Users.Services {
    public class PasswordService : IPasswordService {
        private const string DefaultHashAlgorithm = PasswordExtensions.PBKDF2;
        private readonly IEncryptionService _encryptionService;
        private readonly IAppConfigurationAccessor _appConfigurationAccessor;

        public PasswordService(
            IEncryptionService encryptionService,
            IAppConfigurationAccessor appConfigurationAccessor) {
            _encryptionService = encryptionService;
            _appConfigurationAccessor = appConfigurationAccessor;
        }

        public bool IsMatch(PasswordContext context, string plaintextPassword) {
            switch (context.PasswordFormat) {
                case MembershipPasswordFormat.Clear:
                    return EqualsClear(context, plaintextPassword);
                case MembershipPasswordFormat.Hashed:
                    return EqualsHashed(context, plaintextPassword);
                case MembershipPasswordFormat.Encrypted:
                    return EqualsEncrypted(context, plaintextPassword);
                default:
                    throw new ApplicationException("Unexpected password format value");
            }
        }

        private bool EqualsClear(PasswordContext context, string plaintextPassword) {
            return context.Password == plaintextPassword;
        }
        private bool EqualsHashed(PasswordContext context, string plaintextPassword) {
            var saltBytes = Convert.FromBase64String(context.PasswordSalt);

            bool isValid;
            if (context.HashAlgorithm == PasswordExtensions.PBKDF2) {
                // We can't reuse ComputeHashBase64 as the internally generated salt repeated calls to Crypto.HashPassword() return different results.
                isValid = Crypto.VerifyHashedPassword(context.Password, Encoding.Unicode.GetString(PasswordExtensions.CombineSaltAndPassword(saltBytes, plaintextPassword)));
            }
            else {
                isValid = SecureStringEquality(context.Password, PasswordExtensions.ComputeHashBase64(context.HashAlgorithm, saltBytes, plaintextPassword));
            }

            // Migrating older password hashes to Default algorithm if necessary and enabled.
            if (isValid && context.HashAlgorithm != DefaultHashAlgorithm) {
                var keepOldConfiguration = _appConfigurationAccessor.GetConfiguration("Orchard.Users.KeepOldPasswordHash");
                if (String.IsNullOrEmpty(keepOldConfiguration) || keepOldConfiguration.Equals("false", StringComparison.OrdinalIgnoreCase)) {
                    context.HashAlgorithm = DefaultHashAlgorithm;
                    context.Password = PasswordExtensions.ComputeHashBase64(context.HashAlgorithm, saltBytes, plaintextPassword);
                }
            }

            return isValid;
        }
        private bool EqualsEncrypted(PasswordContext context, string plaintextPassword) {
            return String.Equals(plaintextPassword, Encoding.UTF8.GetString(_encryptionService.Decode(Convert.FromBase64String(context.Password))), StringComparison.Ordinal);
        }


        /// <summary>
        /// Compares two strings without giving hint about the time it takes to do so.
        /// </summary>
        /// <param name="a">The first string to compare.</param>
        /// <param name="b">The second string to compare.</param>
        /// <returns><c>true</c> if both strings are equal, <c>false</c>.</returns>
        private bool SecureStringEquality(string a, string b) {
            if (a == null || b == null || (a.Length != b.Length)) {
                return false;
            }

            var aBytes = Encoding.Unicode.GetBytes(a);
            var bBytes = Encoding.Unicode.GetBytes(b);

            var bytesAreEqual = true;
            for (int i = 0; i < a.Length; i++) {
                bytesAreEqual &= (aBytes[i] == bBytes[i]);
            }

            return bytesAreEqual;
        }

    }
}