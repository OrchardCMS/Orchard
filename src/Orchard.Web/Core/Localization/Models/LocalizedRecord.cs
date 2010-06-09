using Orchard.ContentManagement.Records;

namespace Orchard.Core.Localization.Models {
    public class LocalizedRecord : ContentPartRecord {
        public virtual int CultureId { get; set; }
        public virtual int MasterContentItemId { get; set; }
    }
}