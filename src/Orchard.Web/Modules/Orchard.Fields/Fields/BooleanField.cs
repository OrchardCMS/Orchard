using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;

namespace Orchard.Fields.Fields {
    public class BooleanField : ContentField {

        public Boolean? Value {
            get { return Storage.Get<Boolean?>(); }

            set { Storage.Set(value); }
        }
    }
}
