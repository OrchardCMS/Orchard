using Orchard.ContentManagement.Records;

namespace Orchard.AntiSpam.Models {
    public class AkismetSettingsPartRecord : ContentPartRecord {

        public virtual bool TrustAuthenticatedUsers { get; set; }

        // API key for the Akismet (http://akismet.com/personal/)
        public virtual string ApiKey { get; set; }
    }
}