using System.Collections.Specialized;
using System.Web.Mvc;
using Orchard.ContentManagement;

namespace Orchard.DynamicForms.Services {
    public class WriteElementValuesContext {
        public WriteElementValuesContext() {
        }
        public IValueProvider ValueProvider { get; set; }
        public NameValueCollection Output { get; set; }
        public IContent Content { get; set; }
    }
}