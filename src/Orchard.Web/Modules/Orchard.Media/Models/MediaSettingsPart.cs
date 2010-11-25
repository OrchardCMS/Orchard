using Orchard.ContentManagement;

namespace Orchard.Media.Models {
    public class MediaSettingsPart : ContentPart<MediaSettingsPartRecord> {
        public string UploadAllowedFileTypeWhitelist {
            get { return Record.UploadAllowedFileTypeWhitelist; }
            set { Record.UploadAllowedFileTypeWhitelist = value; }
        }
    }
}