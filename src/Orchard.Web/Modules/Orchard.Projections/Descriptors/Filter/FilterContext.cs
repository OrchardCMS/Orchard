using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Projections.Descriptors.Filter {
    public class FilterContext {
        public FilterContext() {
            Tokens = new Dictionary<string, object>();
            VersionScope = QueryVersionScopeOptions.Published;
        }

        public IDictionary<string, object> Tokens { get; set; }
        public dynamic State { get; set; }
        public IHqlQuery Query { get; set; }

        public QueryVersionScopeOptions VersionScope { get; set; }
    }
}