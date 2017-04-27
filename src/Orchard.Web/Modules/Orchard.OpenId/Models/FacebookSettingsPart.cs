using System;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Orchard.OpenId.Models {
    [OrchardFeature("Orchard.OpenId.Facebook")]
    public class FacebookSettingsPart : ContentPart {

        public string AppId {
            get { return this.Retrieve(x => x.AppId); }
            set { this.Store(x => x.AppId, value); }
        }

        public string AppSecret {
            get { return this.Retrieve(x => x.AppSecret); }
            set { this.Store(x => x.AppSecret, value); }
        }

        public bool IsValid() {
            if (String.IsNullOrWhiteSpace(AppId) ||
                String.CompareOrdinal(AppId, Constants.Facebook.DefaultAppId) == 0 ||
                String.IsNullOrWhiteSpace(AppSecret) ||
                String.CompareOrdinal(AppSecret, Constants.Facebook.DefaultAppSecret) == 0) {

                return false;
            }

            return true;
        }
    }
}