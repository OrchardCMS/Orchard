using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace Orchard.Warmup.Models {
    public class WarmupSettingsPartRecord : ContentPartRecord {
        public WarmupSettingsPartRecord() {
            Delay = 90;
        }

        [StringLengthMax]
        public virtual string Urls { get; set; }
        public virtual bool Scheduled { get; set; }
        public virtual int Delay { get; set; }
        public virtual bool OnPublish { get; set; }
    }
}