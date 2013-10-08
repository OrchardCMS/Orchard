using Orchard.ContentManagement;

namespace Orchard.SecureSocketsLayer.Models {
    public class SslSettingsPart : ContentPart<SslSettingsPartRecord> {
        public string Urls
        {
            get { return Record.Urls; }
            set { Record.Urls = value; }
        }

        public bool SecureEverything {
            get { return Record.SecureEverything; }
            set { Record.SecureEverything = value; }
        }

        public bool CustomEnabled {
            get { return Record.CustomEnabled; }
            set { Record.CustomEnabled = value; }
        }

        public string SecureHostName {
            get { return Record.SecureHostName; } 
            set { Record.SecureHostName = value; }
        }

        public string InsecureHostName {
            get { return Record.InsecureHostName; }
            set { Record.InsecureHostName = value; }
        }
    }
}