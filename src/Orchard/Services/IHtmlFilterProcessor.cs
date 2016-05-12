using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Services
{
    public interface IHtmlFilterProcessor : IDependency {
        string ProcessContent(string text, HtmlFilterContext context);
    }

    public static class HtmlFilterProcessorExtensions {
        public static string ProcessContent(this IHtmlFilterProcessor processor, string text, string flavor, IDictionary<string, object> data)
        {
            return processor.ProcessContent(text, new HtmlFilterContext { Flavor = flavor, Data = data });
        }

        public static string ProcessContent(this IHtmlFilterProcessor processor, string text, string flavor) {
            return processor.ProcessContent(text, new HtmlFilterContext { Flavor = flavor });
        }

        public static string ProcessContent(this IHtmlFilterProcessor processor, string text, string flavor, IContent content) {
            return processor.ProcessContent(text, new HtmlFilterContext { Flavor = flavor, Data = new Dictionary<string, object> { { "Content", content.ContentItem } } });
        }
    }
}