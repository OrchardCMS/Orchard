using Orchard.ContentManagement;

namespace Orchard.AntiSpam.Models {
    public class ReCaptchaSettingsPart : ContentPart<ReCaptchaSettingsPartRecord> {
        public string PublicKey {
            get { return Record.PublicKey; }
            set { Record.PublicKey = value; }
        }

        public string PrivateKey {
            get { return Record.PrivateKey; }
            set { Record.PrivateKey = value; }
        }

        public bool TrustAuthenticatedUsers {
            get { return Record.TrustAuthenticatedUsers; }
            set { Record.TrustAuthenticatedUsers = value; }
        }
    }
}