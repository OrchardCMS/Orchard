using Orchard.ContentManagement;

namespace Orchard.AntiSpam.Models {
    public class AkismetSettingsPart : ContentPart {
        public bool TrustAuthenticatedUsers {
            get { return bool.Parse(Get("TrustAuthenticatedUsers") ?? "false"); }
            set { Set("TrustAuthenticatedUsers", value.ToString()); }
        }

        public string ApiKey {
            get { return Get("ApiKey"); }
            set { Set("ApiKey", value); }
        }
    }
}