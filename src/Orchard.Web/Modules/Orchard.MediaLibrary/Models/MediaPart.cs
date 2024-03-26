using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Utilities;
using Orchard.Core.Title.Models;
using Orchard.MediaLibrary.Handlers;

namespace Orchard.MediaLibrary.Models {
    public class MediaPart : ContentPart<MediaPartRecord> {

        internal LazyField<string> _publicUrl = new LazyField<string>();

        /// <summary>
        /// Gets or sets the title of the media.
        /// This adds an implicit dependency on <see cref="TitlePart"/> which will be resolved by an
        /// <see cref="ActivatingFilter{TPart}"/> in the <see cref="MediaPartHandler"/>.
        /// </summary>
        public string Title {
            get { return ContentItem.As<TitlePart>().Title; }
            set { ContentItem.As<TitlePart>().Title = value; }
        }

        /// <summary>
        /// Gets or sets the mime type of the media.
        /// </summary>
        public string MimeType {
            get { return Retrieve(x => x.MimeType); }
            set { Store(x => x.MimeType, value); }
        }

        /// <summary>
        /// Gets or sets the caption of the media.
        /// </summary>
        public string Caption {
            get { return Retrieve(x => x.Caption); }
            set { Store(x => x.Caption, value); }
        }

        /// <summary>
        /// Gets or sets the alternate text of the media.
        /// </summary>
        public string AlternateText {
            get { return Retrieve(x => x.AlternateText); }
            set { Store(x => x.AlternateText, value); }
        }

        /// <summary>
        /// Gets or sets the hierarchical location of the media.
        /// </summary>
        public string FolderPath {
            get { return Retrieve(x => x.FolderPath); }
            set { Store(x => x.FolderPath, value); }
        }

        /// <summary>
        /// Gets or sets the name of the media when <see cref="IMediaService"/> is used 
        /// to store the physical media. If <value>null</value> then the media is not associated
        /// with a local file.
        /// </summary>
        public string FileName {
            get { return Retrieve(x => x.FileName); }
            set { Store(x => x.FileName, value); }
        }

        /// <summary>
        /// Gets the public Url of the media if stored locally.
        /// </summary>
        public string MediaUrl {
            get { return _publicUrl.Value; }
        }

        /// <summary>
        /// Get or sets the logical type of the media. For instance a custom type could be rendered as an Image
        /// </summary>
        /// <remarks>
        /// The logical type is used to drive the thumbnails generation in the admin.
        /// </remarks>
        public string LogicalType {
            get { return Convert.ToString(this.As<InfosetPart>().Get<MediaPart>("LogicalType")); }
            set { this.As<InfosetPart>().Set<MediaPart>("LogicalType", value); }
        }
    }
}