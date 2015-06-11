using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using IDeliverable.LicensingService.Exceptions;

namespace IDeliverable.LicensingService.Models
{
    public class LicenseValidationToken
    {
        public static LicenseValidationToken Create(LicenseValidationInfo info, X509Certificate2 cert)
        {
            var data = info.ToString();
            var bytes = Encoding.UTF8.GetBytes(data);
            var privateKey = (RSACryptoServiceProvider)cert.PrivateKey;
            var signatureBytes = privateKey.SignData(bytes, "md5");
            var signature = Convert.ToBase64String(signatureBytes);

            return new LicenseValidationToken(info, signature);
        }

        public static LicenseValidationToken CreateInvalidLicenseToken(LicenseValidationError error) => new LicenseValidationToken(error);

        public LicenseValidationError? Error { get; private set; }
        public LicenseValidationInfo Info { get; private set; }
        public string Signature { get; private set; }

        public LicenseValidationToken(LicenseValidationInfo info, string signature)
        {
            Info = info;
            Signature = signature;
        }

        private LicenseValidationToken(LicenseValidationError error)
        {
            Error = error;
        }
    }
}