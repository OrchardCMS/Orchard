using Orchard.ContentManagement.Records;

namespace Orchard.AntiSpam.Models {
    public class SpamFilterPartRecord : ContentPartRecord {
        public virtual SpamStatus Status { get; set; }
    }
}