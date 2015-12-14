using Orchard.ContentManagement;

namespace Orchard.AntiSpam.Models {
    public class SpamFilterPart : ContentPart<SpamFilterPartRecord> {

        public SpamStatus Status {
            get { return Record.Status; }
            set { Record.Status = value; }
        }
    }
}