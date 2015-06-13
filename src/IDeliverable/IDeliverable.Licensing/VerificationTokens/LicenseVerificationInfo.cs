using System;
using Newtonsoft.Json;

namespace IDeliverable.Licensing.VerificationTokens
{
    public class LicenseVerificationInfo
    {
        public LicenseVerificationInfo(int productId, string hostname, string licenseKey, long verifiedUtcTicks)
        {
            ProductId = productId;
            Hostname = hostname;
            LicenseKey = licenseKey;
            VerifiedUtcTicks = verifiedUtcTicks;
        }

        public int ProductId { get; private set; }
        public string Hostname { get; private set; }
        public string LicenseKey { get; private set; }
        public long VerifiedUtcTicks { get; private set; }

        public DateTime VerifiedUtc => new DateTime(VerifiedUtcTicks, DateTimeKind.Utc);
        public TimeSpan Age => DateTime.UtcNow - VerifiedUtc;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}