using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;

namespace Orchard.Fields.Fields
{
    public class BooleanField : ContentField
    {

        public bool? Value
        {
            get { return Storage.Get<bool?>(); }

            set { Storage.Set(value); }
        }
    }
}
