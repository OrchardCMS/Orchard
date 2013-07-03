using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using Orchard.Core.Title.Models;

namespace Orchard.MediaLibrary.Models {
    public class MediaPart : ContentPart<MediaPartRecord> {

        internal LazyField<string> _publicUrl = new LazyField<string>();

        /// <summary>
        /// Gets or sets the title of the media.
        /// </summary>
        public string Title {
            get { return ContentItem.As<TitlePart>().Title; }
            set { ContentItem.As<TitlePart>().Title = value; }
        }

        /// <summary>
        /// Gets or sets the mime type of the media.
        /// </summary>
        public string MimeType {
            get { return Record.MimeType; }
            set { Record.MimeType = value; }
        }

        /// <summary>
        /// Gets or sets the caption of the media.
        /// </summary>
        public string Caption {
            get { return Record.Caption; }
            set { Record.Caption = value; }
        }

        /// <summary>
        /// Gets or sets the alternate text of the media.
        /// </summary>
        public string AlternateText {
            get { return Record.AlternateText; }
            set { Record.AlternateText = value; }
        }

        /// <summary>
        /// Gets or sets the hierarchical location of the media.
        /// </summary>
        public string FolderPath {
            get { return Record.FolderPath; }
            set { Record.FolderPath = value; }
        }

        /// <summary>
        /// Gets or set the name of the media when <see cref="IMediaService"/> is used 
        /// to store the physical media. If <value>null</value> then the media is not associated
        /// with a local file.
        /// </summary>
        public string FileName {
            get { return Record.FileName; }
            set { Record.FileName = value; }
        }

        /// <summary>
        /// Gets the public Url of the media if stored locally.
        /// </summary>
        public string MediaUrl {
            get { return _publicUrl.Value;  }
        }
    }
}