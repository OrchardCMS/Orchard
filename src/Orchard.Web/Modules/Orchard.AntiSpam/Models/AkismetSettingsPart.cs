using Orchard.ContentManagement;

namespace Orchard.AntiSpam.Models {
    public class AkismetSettingsPart : ContentPart {
        public bool TrustAuthenticatedUsers {
            get { return this.Retrieve(x => x.TrustAuthenticatedUsers); }
            set { this.Store(x => x.TrustAuthenticatedUsers, value); }
        }

        public string ApiKey {
            get { return this.Retrieve(x => x.ApiKey); }
            set { this.Store(x => x.ApiKey, value); }
        }
    }
}