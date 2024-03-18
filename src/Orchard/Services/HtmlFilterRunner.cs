using System.Collections.Generic;
using System.Linq;

namespace Orchard.Services {
    public class HtmlFilterRunner : IHtmlFilterRunner {
        private readonly IEnumerable<IHtmlFilter> _filters;
        public HtmlFilterRunner(IEnumerable<IHtmlFilter> filters) {
            _filters = filters;
        }

        public string RunFilters(string text, HtmlFilterContext context) {
            return _filters.Aggregate(text, (current, htmlFilter) => htmlFilter.ProcessContent(current, context));
        }
    }
}