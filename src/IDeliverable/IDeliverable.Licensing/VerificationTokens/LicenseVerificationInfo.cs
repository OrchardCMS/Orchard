using System;
using Newtonsoft.Json;

namespace IDeliverable.Licensing.VerificationTokens
{
    public class LicenseVerificationInfo
    {
        public LicenseVerificationInfo(string productId, string hostname, string licenseKey, long verifiedUtcTicks)
        {
            ProductId = productId;
            Hostname = hostname;
            LicenseKey = licenseKey;
            VerifiedUtcTicks = verifiedUtcTicks;
        }

        public string ProductId { get; }
        public string Hostname { get; }
        public string LicenseKey { get; }
        public long VerifiedUtcTicks { get; }

        public DateTime VerifiedUtc => new DateTime(VerifiedUtcTicks, DateTimeKind.Utc);
        public TimeSpan Age => DateTime.UtcNow - VerifiedUtc;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(
                new
                {
                    ProductId,
                    Hostname,
                    LicenseKey,
                    VerifiedUtcTicks
                });
        }
    }
}