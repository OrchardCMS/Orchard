using Orchard.ContentManagement.Records;

namespace Orchard.Localization.Models {
    public class LocalizationPartRecord : ContentPartRecord {
        public virtual int CultureId { get; set; }
        public virtual int MasterContentItemId { get; set; }
    }
}