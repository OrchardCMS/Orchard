using System;
using System.Collections.Generic;

namespace IDeliverable.Licensing.Orchard.Models
{
    public class LicenseFile
    {
        public static LicenseFile CreateNullInstance(string extensionName)
        {
            return new LicenseFile(extensionName);
        }

        public LicenseFile(string extensionName)
        {
            ExtensionName = extensionName;
            Settings = new Dictionary<string, string>();
        }

        public string ExtensionName { get; set; }
        public IDictionary<string, string> Settings { get; set; }

        public string LicenseKey
        {
            get { return GetValue("LicenseKey"); }
            set { SetValue("LicenseKey", value); }
        }

        public string EncodedValidationToken
        {
            get { return GetValue("ValidationToken"); }
            set { SetValue("ValidationToken", value); }
        }

        public LicenseValidationToken GetLicenseValidationToken()
        {
            return String.IsNullOrWhiteSpace(EncodedValidationToken)
                ? LicenseValidationToken.Empty
                : LicenseValidationToken.Decode(EncodedValidationToken);
        }

        public void SetLicenseValidationToken(LicenseValidationToken token)
        {
            EncodedValidationToken = token.Encode();
        }

        public ILicense GetLicense(ProductManifest productManifest)
        {
            return new License
            {
                ProductId = productManifest.ProductId,
                Key = LicenseKey
            };
        }

        private string GetValue(string key)
        {
            return Settings.ContainsKey(key) ? Settings[key] : null;
        }

        private void SetValue(string key, string value)
        {
            Settings[key] = value;
        }
    }
}