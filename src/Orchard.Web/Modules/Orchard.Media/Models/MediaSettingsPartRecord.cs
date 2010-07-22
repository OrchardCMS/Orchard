using Orchard.ContentManagement.Records;

namespace Orchard.Media.Models {
    public class MediaSettingsPartRecord : ContentPartRecord {
        public virtual string RootMediaFolder { get; set; }
    }
}