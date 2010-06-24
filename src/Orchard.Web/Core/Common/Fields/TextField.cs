using Orchard.ContentManagement;

namespace Orchard.Core.Common.Fields {
    public class TextField : ContentField {
        public string Value {
            get { return Getter(null); }
            set { Setter(null, value); }
        }
    }
}
