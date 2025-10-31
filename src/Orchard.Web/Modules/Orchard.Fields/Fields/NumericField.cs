using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;

namespace Orchard.Fields.Fields
{
    public class NumericField : ContentField
    {

        public decimal? Value
        {
            get { return Storage.Get<decimal?>(); }
            set { Storage.Set(value); }
        }
    }
}
