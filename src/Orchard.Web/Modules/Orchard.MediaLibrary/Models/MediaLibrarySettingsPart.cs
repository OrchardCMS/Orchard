using System;
using System.Linq;
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

        public bool IsFileAllowed(string filename) {

            var allowedExtensions = (UploadAllowedFileTypeWhitelist ?? "")
                .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x.StartsWith("."))
                .ToArray();

            // skip file if the allowed extensions is defined and doesn't match
            if (allowedExtensions.Any()) {
                if (!allowedExtensions.Any(e => filename.EndsWith(e, StringComparison.OrdinalIgnoreCase))) {
                    return false;
                }
            }

            // web.config files are always ignored, even if the white list includes it
            if (String.Equals(filename, "web.config", StringComparison.OrdinalIgnoreCase)) {
                return false;
            }

            return true;
        }
    }
}