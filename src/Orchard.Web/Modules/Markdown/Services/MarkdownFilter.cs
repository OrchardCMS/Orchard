using System;
using Orchard.Services;

namespace Markdown.Services
{
    public class MarkdownFilter : HtmlFilter {
        public override string ProcessContent(string text, HtmlFilterContext context) {
            return String.Equals(context.Flavor, "markdown", StringComparison.OrdinalIgnoreCase) ? MarkdownReplace(text) : text;
        }

        private static string MarkdownReplace(string text) {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            return Markdig.Markdown.ToHtml(text);
        }
    }
}
