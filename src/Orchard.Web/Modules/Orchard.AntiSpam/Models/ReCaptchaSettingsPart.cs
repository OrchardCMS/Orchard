using Orchard.ContentManagement;

namespace Orchard.AntiSpam.Models {
    public class ReCaptchaSettingsPart : ContentPart {
        public string PublicKey {
            get { return this.Retrieve(x => x.PublicKey); }
            set { this.Store(x => x.PublicKey, value); }
        }

        public string PrivateKey {
            get { return this.Retrieve(x => x.PrivateKey); }
            set { this.Store(x => x.PrivateKey, value); }
        }

        public bool TrustAuthenticatedUsers {
            get { return this.Retrieve(x => x.TrustAuthenticatedUsers); }
            set { this.Store(x => x.TrustAuthenticatedUsers, value); }
        }
    }
}