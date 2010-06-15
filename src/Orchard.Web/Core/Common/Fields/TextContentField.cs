using Orchard.ContentManagement;

namespace Orchard.Core.Common.Fields {
    public class TextContentField : ContentField {
        public string TextField {
            get { return Getter("text"); }
            set { Setter("text", value); }
        }
    }
}
