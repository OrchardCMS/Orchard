using System;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Orchard.OpenId.Models {
    [OrchardFeature("Orchard.OpenId.Google")]
    public class GoogleSettingsPart : ContentPart {

        public string ClientId {
            get { return this.Retrieve(x => x.ClientId, () => Constants.Google.DefaultClientId); }
            set { this.Store(x => x.ClientId, value); }
        }

        public string ClientSecret {
            get { return this.Retrieve(x => x.ClientSecret, () => Constants.Google.DefaultClientSecret); }
            set { this.Store(x => x.ClientSecret, value); }
        }

        public string CallbackPath {
            get { return this.Retrieve(x => x.CallbackPath, () => Constants.General.LogonCallbackUrl); }
            set { this.Store(x => x.CallbackPath, value); }
        }

        public bool IsValid {
            get {
                if (String.IsNullOrWhiteSpace(ClientId) ||
                    String.CompareOrdinal(ClientId, Constants.Google.DefaultClientId) == 0 ||
                    String.IsNullOrWhiteSpace(ClientSecret) ||
                    String.CompareOrdinal(ClientId, Constants.Google.DefaultClientSecret) == 0 ||
                    String.IsNullOrWhiteSpace(CallbackPath)) {

                    return false;
                }

                return true;
            }
        }
    }
}