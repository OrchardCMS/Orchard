using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using System;

namespace Orchard.Fields.Fields {
    public class BooleanField : ContentField {
        internal LazyField<Boolean?> _valueField = new LazyField<Boolean?>();

        public Boolean? Value {
            get { return _valueField.Value; }

            set { _valueField.Value = value; }
        }
    }
}
