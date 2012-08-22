using Orchard.ContentManagement.Records;

namespace Orchard.AntiSpam.Models {
    public class TypePadSettingsPartRecord : ContentPartRecord {

        public virtual bool TrustAuthenticatedUsers { get; set; }

        // API key for TypePad AntiSpam (http://antispam.typepad.com/info/get-api-key.html).
        public virtual string ApiKey { get; set; }
    }
}