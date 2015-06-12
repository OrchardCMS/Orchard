using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;

namespace IDeliverable.Licensing
{
    public class LicenseValidationToken
    {
        public static readonly LicenseValidationToken Empty = new LicenseValidationToken(LicenseValidationInfo.Empty, signature: "");

        public static LicenseValidationToken CreateLocalHostToken(int productId)
        {
            return new LicenseValidationToken(new LicenseValidationInfo
            (
                productId,
                hostname: "localhost",
                licenseKey: "localhost",
                issuedUtcTicks: DateTime.UtcNow.Ticks
            ),
            signature: "");
        }

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

        public static LicenseValidationToken Parse(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return Empty;

            return JsonConvert.DeserializeObject<LicenseValidationToken>(value);
        }

        public static LicenseValidationToken Decode(string value)
        {
            var bytes = Convert.FromBase64String(value);
            var json = Encoding.UTF8.GetString(bytes);
            var token = Parse(json);
            
            return token;
        }

        public LicenseValidationToken(LicenseValidationInfo info, string signature)
        {
            Info = info;
            Signature = signature;
        }

        private LicenseValidationToken(LicenseValidationError error)
        {
            Error = error;
        }

        public LicenseValidationError? Error { get; set; }
        public LicenseValidationInfo Info { get; set; }
        public string Signature { get; set; }
        public DateTime IssuedUtc => new DateTime(Info?.IssuedUtcTicks ?? 0, DateTimeKind.Utc);
        public bool IsLocalHost => Info?.Hostname == "localhost";
        public bool IsEmpty => Info.IsEmpty && Signature == String.Empty;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public TimeSpan IssuedSince()
        {
            return DateTime.Now - IssuedUtc;
        }

        public string Encode()
        {
            var bytes = Encoding.UTF8.GetBytes(ToString());
            var base64 = Convert.ToBase64String(bytes);

            return base64;
        }
    }
}