using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;

namespace Orchard.Fields.Fields {
    public class MediaPickerField : ContentField {
        public string Url {
            get { return Storage.Get<string>(); }
            set { Storage.Set(value); }
        }

        public string AlternateText {
            get { return Storage.Get<string>("AlternateText"); }
            set { Storage.Set("AlternateText", value); }
        }

        public string Class {
            get { return Storage.Get<string>("Class"); }
            set { Storage.Set("Class", value); }
        }

        public string Style {
            get { return Storage.Get<string>("Style"); }
            set { Storage.Set("Style", value); }
        }

        public string Alignment {
            get { return Storage.Get<string>("Alignment"); }
            set { Storage.Set("Alignment", value); }
        }

        public int Width {
            get { return Storage.Get<int>("Width"); }
            set { Storage.Set("Width", value); }
        }

        public int Height {
            get { return Storage.Get<int>("Height"); }
            set { Storage.Set("Height", value); }
        }

    }
}
