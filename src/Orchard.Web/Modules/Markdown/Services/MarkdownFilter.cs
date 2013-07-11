using System;
using Orchard.Services;

namespace Markdown.Services {
    public class MarkdownFilter : IHtmlFilter {
        public string ProcessContent(string text, string flavor) {
            return flavor.Equals("markdown", StringComparison.OrdinalIgnoreCase) ? MarkdownReplace(text) : text;
        }

        private static string MarkdownReplace(string text) {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var markdown = new MarkdownSharp.Markdown();
            return markdown.Transform(text);
        }
    }
}