using System;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Orchard.OpenId.Models
{
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

        [RegularExpression(pattern: "/.+", ErrorMessage = "The Callback Path Must start with a forward slash '/' followed by one or more characters")]
        public string CallbackPath {
            get { return this.Retrieve(x => x.CallbackPath, () => Constants.General.LogonCallbackUrl); }
            set { this.Store(x => x.CallbackPath, value); }
        }

        public bool IsValid() {
            if (String.IsNullOrWhiteSpace(ClientId) ||
                String.CompareOrdinal(ClientId, Constants.Google.DefaultClientId) == 0 ||
                String.IsNullOrWhiteSpace(ClientSecret) ||
                String.CompareOrdinal(ClientId, Constants.Google.DefaultClientSecret) == 0 ||
                String.IsNullOrWhiteSpace(CallbackPath) ||
                CallbackPath.StartsWith("/") == false ||
                CallbackPath.Length < 2) {

                return false;
            }

            return true;
        }
    }
}