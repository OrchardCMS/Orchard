using System.Collections.Generic;

namespace Orchard.Services
{
    public class HtmlFilterContext {
        public HtmlFilterContext() {
            Data = new Dictionary<string, object>();
        }

        public string Flavor { get; set; }
        public IDictionary<string, object> Data { get; set; }
    }
}