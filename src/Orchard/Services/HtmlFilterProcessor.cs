using System.Collections.Generic;

namespace Orchard.Services {
    public class HtmlFilterProcessor : IHtmlFilterProcessor {
#pragma warning disable CS0618 // Type or member is obsolete
        private readonly IEnumerable<IHtmlFilter> _filters;
        public HtmlFilterProcessor(IEnumerable<IHtmlFilter> filters) {
#pragma warning restore CS0618 // Type or member is obsolete
            _filters = filters;
        }

        public string ProcessContent(string text, HtmlFilterContext context) {
            foreach (var htmlFilter in _filters) {
                var htmlFilter2 = htmlFilter as IHtmlFilter2;
                text = htmlFilter2 != null 
                    ? htmlFilter2.ProcessContent(text, context) // Execute the newer version of the interface.
                    : htmlFilter.ProcessContent(text, context.Flavor); // Fall back to the currently deprecated version of the interface.
            }
            return text;
        }
    }
}