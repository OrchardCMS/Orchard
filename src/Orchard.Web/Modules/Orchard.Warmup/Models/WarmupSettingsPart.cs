using Orchard.ContentManagement;

namespace Orchard.Warmup.Models {
    public class WarmupSettingsPart : ContentPart<WarmupSettingsPartRecord> {

        public string Urls {
            get { return Record.Urls; }
            set { Record.Urls = value; }
        }

        public bool Scheduled {
            get { return Record.Scheduled; }
            set { Record.Scheduled = value; }
        }

        public int Delay {
            get { return Record.Delay; }
            set { Record.Delay = value; }
        }

        public bool OnPublish {
            get { return Record.OnPublish; }
            set { Record.OnPublish = value; }
        }
    }
}