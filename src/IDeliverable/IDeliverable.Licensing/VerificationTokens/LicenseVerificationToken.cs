using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IDeliverable.Licensing.VerificationTokens
{
    public class LicenseVerificationToken
    {
        public static LicenseVerificationToken Create(LicenseVerificationInfo info, X509Certificate2 signingCert)
        {
            var infoString = info.ToString();
            var infoBytes = Encoding.UTF8.GetBytes(infoString);
            var privateKey = (RSACryptoServiceProvider)signingCert.PrivateKey;
            var signatureBytes = privateKey.SignData(infoBytes, "md5");
            var signature = Convert.ToBase64String(signatureBytes);

            return new LicenseVerificationToken(info, signature);
        }

        public static LicenseVerificationToken FromBase64(string value)
        {
            var tokenBytes = Convert.FromBase64String(value);
            var tokenJson = Encoding.UTF8.GetString(tokenBytes);
            return Parse(tokenJson);
        }

        public static LicenseVerificationToken Parse(string value)
        {
            return JsonConvert.DeserializeObject<LicenseVerificationToken>(value, new StringEnumConverter());
        }

        public LicenseVerificationToken(LicenseVerificationInfo info, string signature)
        {
            if (info == null)
                throw new ArgumentNullException("info", $"The parameter {nameof(info)} cannot be null.");

            Info = info;
            Signature = signature;
        }

        public LicenseVerificationInfo Info { get; private set; }
        public string Signature { get; private set; }

        public DateTime VerifiedUtc => Info.VerifiedUtc; // Delegated to Info because it needs to be included in signature.
        public TimeSpan Age => Info.Age; // Delegated to Info because it needs to be included in signature.

        public string ToBase64()
        {
            var tokenJson = ToString();
            var tokenBytes = Encoding.UTF8.GetBytes(tokenJson);
            return Convert.ToBase64String(tokenBytes);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public bool GetSignatureIsValid(X509Certificate2 signingCert)
        {
            var publicKey = (RSACryptoServiceProvider)signingCert.PublicKey.Key;
            var infoBytes = Encoding.UTF8.GetBytes(Info.ToString());
            var signatureBytes = Convert.FromBase64String(Signature);
            var isValid = publicKey.VerifyData(infoBytes, "md5", signatureBytes);

            return isValid;
        }
    }
}