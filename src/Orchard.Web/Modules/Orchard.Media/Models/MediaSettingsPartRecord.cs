using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement.Records;

namespace Orchard.Media.Models {
    public class MediaSettingsPartRecord : ContentPartRecord {
        internal const string DefaultWhitelist = "jpg jpeg gif png txt doc docx xls xlsx pdf ppt pptx pps ppsx odt ods odp";
        private string _whitelist = DefaultWhitelist;

        [StringLength(255)]
        public virtual string UploadAllowedFileTypeWhitelist {
            get { return _whitelist; }
            set { _whitelist = value; }
        }
    }
}