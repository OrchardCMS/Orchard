using System.Collections.Specialized;
using System.Web.Mvc;

namespace Orchard.DynamicForms.Services {
    public class WriteElementValuesContext {
        public WriteElementValuesContext() {
        }
        public IValueProvider ValueProvider { get; set; }
    }
}