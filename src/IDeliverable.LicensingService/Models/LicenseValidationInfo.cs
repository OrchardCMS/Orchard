using System;
using Newtonsoft.Json;

namespace IDeliverable.LicensingService.Models
{
    public class LicenseValidationInfo
    {
        public static LicenseValidationInfo Parse(string value)
        {
            return JsonConvert.DeserializeObject<LicenseValidationInfo>(value);
        }

        public LicenseValidationInfo(int productId, string hostname, string licenseKey, long issuedUtcTicks)
        {
            ProductId = productId;
            Hostname = hostname;
            LicenseKey = licenseKey;
            IssuedUtcTicks = issuedUtcTicks;
        }

        public int ProductId { get; private set; }
        public string Hostname { get; private set; }
        public string LicenseKey { get; private set; }
        public long IssuedUtcTicks { get; private set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}