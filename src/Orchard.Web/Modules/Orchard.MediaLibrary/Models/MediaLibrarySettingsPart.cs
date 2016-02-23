using Orchard.ContentManagement;

namespace Orchard.MediaLibrary.Models {
    public class MediaLibrarySettingsPart : ContentPart {

        /// <summary>
        /// Gets or sets the list of file extensions that can be uploaded
        /// </summary>
        public string UploadAllowedFileTypeWhitelist {
            get { return this.Retrieve(x => x.UploadAllowedFileTypeWhitelist); }
            set { this.Store(x => x.UploadAllowedFileTypeWhitelist, value); }
        }
    }
}