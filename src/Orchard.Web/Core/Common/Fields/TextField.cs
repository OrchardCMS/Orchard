using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;

namespace Orchard.Core.Common.Fields {
    public class TextField : ContentField {
        public string Value {
            get { return Storage.Get<string>(); }
            set { Storage.Set(value); }
        }
    }
}
