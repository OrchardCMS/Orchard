using System;
using Newtonsoft.Json;

namespace IDeliverable.Licensing
{
    public class LicenseValidationInfo
    {
        public static readonly LicenseValidationInfo Empty = new LicenseValidationInfo();

        public int ProductId { get; set; }
        public string Hostname { get; set; }
        public string LicenseKey { get; set; }
        public long IssuedUtcTicks { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}