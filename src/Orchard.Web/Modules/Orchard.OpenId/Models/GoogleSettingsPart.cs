using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Orchard.OpenId.Models {
    [OrchardFeature("Orchard.OpenId.Google")]
    public class GoogleSettingsPart : ContentPart {

        public string ClientId {
            get { return this.Retrieve(x => x.ClientId, () => Constants.DefaultGoogleClientId); }
            set { this.Store(x => x.ClientId, value); }
        }

        public string ClientSecret {
            get { return this.Retrieve(x => x.ClientSecret, () => Constants.DefaultGoogleClientSecret); }
            set { this.Store(x => x.ClientSecret, value); }
        }

        public string CallbackPath {
            get { return this.Retrieve(x => x.CallbackPath, () => Constants.LogonCallbackUrl); }
            set { this.Store(x => x.CallbackPath, value); }
        }

        public bool IsValid {
            get {
                if (string.IsNullOrWhiteSpace(ClientId) ||
                    string.Compare(ClientId, Constants.DefaultGoogleClientId) == 0 ||
                    string.IsNullOrWhiteSpace(ClientSecret) ||
                    string.Compare(ClientId, Constants.DefaultGoogleClientSecret) == 0 ||
                    string.IsNullOrWhiteSpace(CallbackPath)) {

                    return false;
                }

                return true;
            }
        }
    }
}