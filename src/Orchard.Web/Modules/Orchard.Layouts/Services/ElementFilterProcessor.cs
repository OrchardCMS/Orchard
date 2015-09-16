using System.Collections.Generic;
using Orchard.Services;

namespace Orchard.Layouts.Services {
    public class ElementFilterProcessor : IElementFilterProcessor {
        private readonly IEnumerable<IHtmlFilter> _filters;
        public ElementFilterProcessor(IEnumerable<IHtmlFilter> filters) {
            _filters = filters;
        }

        public string ProcessContent(string text, string flavor, IDictionary<string, object> context) {
            foreach (var htmlFilter in _filters) {
                var elementFilter = htmlFilter as IElementFilter;
                text = elementFilter != null ? elementFilter.ProcessContent(text, flavor, context) : htmlFilter.ProcessContent(text, flavor);
            }
            return text;
        }
    }
}