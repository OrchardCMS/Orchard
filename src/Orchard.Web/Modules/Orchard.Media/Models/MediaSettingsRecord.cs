using Orchard.ContentManagement.Records;

namespace Orchard.Media.Models {
    public class MediaSettingsRecord : ContentPartRecord {
        public virtual string RootMediaFolder { get; set; }
    }
}