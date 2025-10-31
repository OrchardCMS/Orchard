using System;
using Orchard.Services;

namespace Markdown.Services
{
    public class MarkdownFilter : HtmlFilter
    {
        public override string ProcessContent(string text, HtmlFilterContext context)
        {
            return string.Equals(context.Flavor, "markdown", StringComparison.OrdinalIgnoreCase) ? MarkdownReplace(text) : text;
        }

        private static string MarkdownReplace(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return Markdig.Markdown.ToHtml(text);
        }
    }
}
