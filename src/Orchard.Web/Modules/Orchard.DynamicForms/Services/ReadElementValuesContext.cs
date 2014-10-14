using System.Collections.Specialized;
using System.Web.Mvc;

namespace Orchard.DynamicForms.Services {
    public class ReadElementValuesContext {
        public ReadElementValuesContext() {
            Output = new NameValueCollection();
        }
        public IValueProvider ValueProvider { get; set; }
        public NameValueCollection Output { get; set; }
    }
}