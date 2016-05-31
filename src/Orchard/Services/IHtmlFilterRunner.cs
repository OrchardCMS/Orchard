using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Services {
    public interface IHtmlFilterRunner : IDependency {
        string RunFilters(string text, HtmlFilterContext context);
    }

    public static class HtmlFilterProcessorExtensions {
        public static string RunFilters(this IHtmlFilterRunner runner, string text, string flavor, IDictionary<string, object> data) {
            return runner.RunFilters(text, new HtmlFilterContext { Flavor = flavor, Data = data });
        }

        public static string RunFilters(this IHtmlFilterRunner runner, string text, string flavor) {
            return runner.RunFilters(text, new HtmlFilterContext { Flavor = flavor });
        }

        public static string RunFilters(this IHtmlFilterRunner runner, string text, string flavor, IContent content) {
            return runner.RunFilters(text, new HtmlFilterContext { Flavor = flavor, Data = new Dictionary<string, object> { { "Content", content.ContentItem } } });
        }
    }
}