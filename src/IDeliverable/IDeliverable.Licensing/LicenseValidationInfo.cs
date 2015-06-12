using System;
using Newtonsoft.Json;

namespace IDeliverable.Licensing
{
    public class LicenseValidationInfo
    {
        public static readonly LicenseValidationInfo Empty = new LicenseValidationInfo(0, String.Empty, String.Empty, 0);

        public LicenseValidationInfo(int productId, string hostname, string licenseKey, long issuedUtcTicks)
        {
            ProductId = productId;
            Hostname = hostname;
            LicenseKey = licenseKey;
            IssuedUtcTicks = issuedUtcTicks;
        }

        public int ProductId { get; set; }
        public string Hostname { get; set; }
        public string LicenseKey { get; set; }
        public long IssuedUtcTicks { get; set; }
        public bool IsEmpty => ProductId == 0 && Hostname == String.Empty && LicenseKey == String.Empty && IssuedUtcTicks == 0;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}