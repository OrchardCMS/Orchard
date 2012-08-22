using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;

namespace Orchard.Fields.Fields {
    public class NumericField : ContentField {

        public Decimal? Value {
            get { return Storage.Get<Decimal?>(); }
            set { Storage.Set(value); }
        }
    }
}
