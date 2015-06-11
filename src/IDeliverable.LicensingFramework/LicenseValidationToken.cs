using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;

namespace IDeliverable.Licensing
{
    public class LicenseValidationToken
    {
        public static readonly LicenseValidationToken Empty = new LicenseValidationToken (LicenseValidationInfo.Empty, signature: "");

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

        public static LicenseValidationToken Deserialize(string value)
        {
            var bytes = Convert.FromBase64String(value);
            var json = Encoding.UTF8.GetString(bytes);
            var token = Parse(json);

            if (!token.Verify(new X509Certificate2(GetCertificate())))
                throw new LicenseValidationException("The specified signature is invalid.", LicenseValidationError.SignatureValidationFailed);

            return token;
        }

        private static byte[] GetCertificate()
        {
            const string cert =
                "-----BEGIN CERTIFICATE-----" +
                "MIIC0jCCAbqgAwIBAgIQEQLRFh6uEYlCFvZSUca9FDANBgkqhkiG9w0BAQUFADAS" +
                "MRAwDgYDVQQDEwdTUy1UNTUwMB4XDTE1MDYwNjIwMDY1NFoXDTE2MDYwNjAwMDAw" +
                "MFowEjEQMA4GA1UEAxMHU1MtVDU1MDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCC" +
                "AQoCggEBAJK3q7E7TDOUp + r64 + 0HLtsjxkloCcBXZMQ / RPaac55 / OzKEUcMBYbhr" +
                "p7rY6Pi5bL2o1YMCFNBB2fSrMux + paZKqTZt3q + c7gewT + ojeplbvliD6wrmXVzf" +
                "Exm04GHJre3qfa1iWA9LheEOMZA4WEC3Rsk2EeqjygWTkb6ADDkYIy4np5DXeBJh" +
                "lB1JqVQNRlP08vP0csCJn23u29sRUNo0azFQO1PStDSdJ9RknYtmWdCp / 1Uk3 / tS" +
                "paG2j3ByfX7WIH +/ aGADigt3a2ixHWnrw / pbBqaiprP0UXbEaI6VeHjdxScoot4l" +
                "6tlTpfEbBSfh + 9ptfuQ1mfiEMHYS0eUCAwEAAaMkMCIwCwYDVR0PBAQDAgQwMBMG" +
                "  A1UdJQQMMAoGCCsGAQUFBwMBMA0GCSqGSIb3DQEBBQUAA4IBAQBTrWWYhsB5pSff" +
                "WSaalC06yu2zPYSAzN1znU9YIx9EUs9xf9 / MRBCkOT7yYMM9ZcLs5DdeBix8vNWI" +
                "iOVw1ZfyWsKNSDnO8 / k6569mVhRLoTXtklMaNsVpk3Bbz3EzFt90j1XFHFWnSIlX" +
                "MXGXetSbirtfBz0fCmxr76UpC7um7Gc6xff2l8S2t4t + mr01Z9w1MTR1nG0aGLLd" +
                "EYOQGRB7Bh / gym45Ro62i0eOflRLfB5S6da2Hg7sqpXtqhlLcyELMvj1x0meW7vH" +
                "B0Jb + L43eRTSN1FmXkx4plUPg6sYIQCa7gEyiT1ExYtGLbdrzJSGj50vbPvEYp + w" +
                "rC5l3KWU" +
                "---- - END CERTIFICATE---- -";

            var bytes = Convert.FromBase64String(cert);
            return bytes;
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
        public TimeSpan RenewalInterval => TimeSpan.FromDays(14);
        public TimeSpan GraceTime => TimeSpan.FromDays(21);
        public bool IsLocalHost => Info?.Hostname == "localhost";

        public bool Verify(X509Certificate2 cert)
        {
            var publicKey = (RSACryptoServiceProvider)cert.PublicKey.Key;
            var infoBytes = Encoding.UTF8.GetBytes(Info.ToString());
            var signatureBytes = Convert.FromBase64String(Signature);
            var isValid = publicKey.VerifyData(infoBytes, "md5", signatureBytes);

            return isValid;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public TimeSpan IssuedSince()
        {
            return DateTime.Now - IssuedUtc;
        }

        public bool GetIsWithinRenewalInterval()
        {
            return IssuedSince() <= RenewalInterval;
        }

        public bool GetIsWithinGraceTime()
        {
            return IssuedSince() > RenewalInterval && IssuedSince() <= GraceTime;
        }

        public bool GetIsExpired()
        {
            return IssuedSince() > GraceTime;
        }

        public string Serialize()
        {
            var bytes = Encoding.UTF8.GetBytes(ToString());
            var base64 = Convert.ToBase64String(bytes);

            return base64;
        }
    }
}