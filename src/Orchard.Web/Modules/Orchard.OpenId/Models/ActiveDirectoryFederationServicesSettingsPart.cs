using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Orchard.OpenId.Models {
    [OrchardFeature("Orchard.OpenId.ActiveDirectoryFederationServices")]
    public class ActiveDirectoryFederationServicesSettingsPart : ContentPart {

        public string ClientId {
            get { return this.Retrieve(x => x.ClientId, () => Constants.DefaultAdfsClientId); }
            set { this.Store(x => x.ClientId, value); }
        }

        public string MetadataAddress {
            get { return this.Retrieve(x => x.MetadataAddress, () => Constants.DefaultAdfsMetadataAddress); }
            set { this.Store(x => x.MetadataAddress, value); }
        }

        public string PostLogoutRedirectUri {
            get { return this.Retrieve(x => x.PostLogoutRedirectUri); }
            set { this.Store(x => x.PostLogoutRedirectUri, value); }
        }

        public bool IsValid {
            get {
                if (string.IsNullOrWhiteSpace(ClientId) ||
                    string.Compare(ClientId, Constants.DefaultAdfsClientId) == 0 ||
                    string.IsNullOrWhiteSpace(MetadataAddress) ||
                    string.Compare(MetadataAddress, Constants.DefaultAdfsMetadataAddress) == 0 ||
                    string.IsNullOrWhiteSpace(PostLogoutRedirectUri)) {

                    return false;
                }

                return true;
            }
        }
    }
}