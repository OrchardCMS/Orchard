using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using IDeliverable.Licensing.Orchard.Models;

namespace IDeliverable.Licensing.Orchard.Services
{
    public class LicenseValidator : ILicenseValidator
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILicenseAccessor _licenseAccessor;
        private readonly ILicenseFileManager _licenseFileManager;

        private static TimeSpan LicenseValidationTokenValidFor => TimeSpan.FromDays(21);

        public LicenseValidator(IHttpContextAccessor httpContextAccessor, ILicenseAccessor licenseAccessor, ILicenseFileManager licenseFileManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _licenseAccessor = licenseAccessor;
            _licenseFileManager = licenseFileManager;
        }

        public ILicense GetLicense(ProductManifest product)
        {
            var licenseFile = _licenseFileManager.Load(product.ExtensionName);
            var license = licenseFile.GetLicense(product);

            return license;
        }

        public LicenseValidationResult ValidateLicense(ProductManifest product, LicenseValidationOptions options = Models.LicenseValidationOptions.Default)
        {
            var license = GetLicense(product);
            return ValidateLicense(license, options);
        }

        public LicenseValidationResult ValidateLicense(ILicense license, LicenseValidationOptions options = Models.LicenseValidationOptions.Default)
        {
            var request = _httpContextAccessor.Current().Request;

            // Skip local requests.
            if ((options & LicenseValidationOptions.SkipLocalRequests) == LicenseValidationOptions.SkipLocalRequests)
                if (request.IsLocal)
                    return LicenseValidationResult.LocalHost();

            var refreshToken = (options & LicenseValidationOptions.RefreshToken) == LicenseValidationOptions.RefreshToken;

            try
            {
                var token = _licenseAccessor.GetLicenseValidationToken(license, refreshToken);

                if (!VerifyTokenSignature(token))
                    throw new LicenseValidationException("The token signature is invalid.", LicenseValidationError.SignatureValidationFailed);

                if (TokenIsExpired(token))
                    return LicenseValidationResult.Invalid(LicenseValidationError.LicenseExpired);

                return LicenseValidationResult.Valid();
            }
            catch (LicenseValidationException ex)
            {
                return LicenseValidationResult.Invalid(ex.Error);
            }
        }

        private static bool TokenIsExpired(LicenseValidationToken token)
        {
            return token.IssuedSince() > LicenseValidationTokenValidFor;
        }

        private static bool VerifyTokenSignature(LicenseValidationToken token)
        {
            var cert = new X509Certificate2(GetCertificate());
            var publicKey = (RSACryptoServiceProvider)cert.PublicKey.Key;
            var infoBytes = Encoding.UTF8.GetBytes(token.Info.ToString());
            var signatureBytes = Convert.FromBase64String(token.Signature);
            var isValid = publicKey.VerifyData(infoBytes, "md5", signatureBytes);

            return isValid;
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
    }
}