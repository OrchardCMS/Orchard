using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;

namespace Orchard.Fields.Fields
{
    public class LinkField : ContentField
    {

        public string Value
        {
            get { return Storage.Get<string>(); }
            set { Storage.Set(value ?? string.Empty); }
        }

        public string Text
        {
            get { return Storage.Get<string>("Text"); }
            set { Storage.Set("Text", value ?? string.Empty); }
        }

        public string Target
        {
            get { return Storage.Get<string>("Target"); }
            set { Storage.Set("Target", value ?? string.Empty); }
        }
    }
}
