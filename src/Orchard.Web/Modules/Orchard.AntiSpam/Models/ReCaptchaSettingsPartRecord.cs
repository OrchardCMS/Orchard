using Orchard.ContentManagement.Records;

namespace Orchard.AntiSpam.Models {
    public class ReCaptchaSettingsPartRecord : ContentPartRecord {
        public virtual string PublicKey { get; set; }
        public virtual string PrivateKey { get; set; }
        public virtual bool TrustAuthenticatedUsers { get; set; }
    }
}