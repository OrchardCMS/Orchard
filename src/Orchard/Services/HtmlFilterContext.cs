using System.Collections.Generic;

namespace Orchard.Services {
    public class HtmlFilterContext {
        public string Flavor { get; set; }
        public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }
}
