using System.Collections.Generic;
using System.Linq;

namespace Orchard.Services {
    public class HtmlFilterProcessor : IHtmlFilterProcessor {
        private readonly IEnumerable<IHtmlFilter> _filters;

        public HtmlFilterProcessor(IEnumerable<IHtmlFilter> filters) {
            _filters = filters;
        }

        public string ProcessFilters(string text, HtmlFilterContext context) {
            return _filters.Aggregate(text, (current, htmlFilter) => htmlFilter.ProcessContent(current, context));
        }
    }
}
