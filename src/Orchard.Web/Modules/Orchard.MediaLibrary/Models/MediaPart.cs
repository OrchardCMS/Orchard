using Orchard.ContentManagement;
using Orchard.Core.Common.Utilities;
using Orchard.Core.Title.Models;
using Orchard.Taxonomies.Models;

namespace Orchard.MediaLibrary.Models {
    public class MediaPart : ContentPart<MediaPartRecord> {

        private readonly LazyField<TermPart> _termPartField = new LazyField<TermPart>();

        public LazyField<TermPart> TermPartField { get { return _termPartField; } }

        public string Title {
            get { return ContentItem.As<TitlePart>().Title; }
            set { ContentItem.As<TitlePart>().Title = value; }
        }

        public string MimeType {
            get { return Record.MimeType; }
            set { Record.MimeType = value; }
        }

        public string Caption {
            get { return Record.Caption; }
            set { Record.Caption = value; }
        }

        public string AlternateText {
            get { return Record.AlternateText; }
            set { Record.AlternateText = value; }
        }

        public TermPart TermPart {
            get { return _termPartField.Value; }
            set { Record.TermPartRecord = value.Record; }
        }

        public string Resource {
            get { return Record.Resource; }
            set { Record.Resource = value; }
        }
    }
}