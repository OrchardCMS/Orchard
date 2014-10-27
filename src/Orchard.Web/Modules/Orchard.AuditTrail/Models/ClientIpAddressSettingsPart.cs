using Orchard.ContentManagement;

namespace Orchard.AuditTrail.Models {
    public class ClientIpAddressSettingsPart : ContentPart {

        public bool EnableClientIpAddressHeader {
            get { return this.Retrieve(x => x.EnableClientIpAddressHeader); }
            set { this.Store(x => x.EnableClientIpAddressHeader, value); }
        }

        public string ClientIpAddressHeaderName {
            get { return this.Retrieve(x => x.ClientIpAddressHeaderName, "X-Forwarded-For"); }
            set { this.Store(x => x.ClientIpAddressHeaderName, value); }
        }
    }
}