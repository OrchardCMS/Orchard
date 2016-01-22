using Orchard.ContentManagement;

namespace Orchard.Warmup.Models {
    public class WarmupSettingsPart : ContentPart {

        public string Urls {
            get { return this.Retrieve(x => x.Urls); }
            set { this.Store(x => x.Urls, value); }
        }

        public bool Scheduled {
            get { return this.Retrieve(x => x.Scheduled); }
            set { this.Store(x => x.Scheduled, value); }
        }

        public int Delay {
            get { return this.Retrieve(x => x.Delay); }
            set { this.Store(x => x.Delay, value); }
        }

        public bool OnPublish {
            get { return this.Retrieve(x => x.OnPublish); }
            set { this.Store(x => x.OnPublish, value); }
        }
    }
}