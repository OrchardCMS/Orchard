using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;

namespace Orchard.Fields.Fields {
    public class InputField : ContentField {

        public string Value {
            get { return Storage.Get<string>(); }
            set { Storage.Set(value ?? String.Empty); }
        }
    }
}
