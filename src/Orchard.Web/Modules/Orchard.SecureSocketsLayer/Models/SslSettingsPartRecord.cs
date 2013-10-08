using Orchard.ContentManagement.Records;

namespace Orchard.SecureSocketsLayer.Models {
    public class SslSettingsPartRecord : ContentPartRecord {
        public virtual string Urls { get; set; }
        public virtual bool SecureEverything { get; set; }
        public virtual bool CustomEnabled { get; set; }
        public virtual string SecureHostName { get; set; }
        public virtual string InsecureHostName { get; set; }
    }
}