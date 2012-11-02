using MarkdownSharp;
using Orchard.Services;

namespace Markdown.Services {
    public class MarkdownFilter : IHtmlFilter {
        public string ProcessContent(string text) {
            return MarkdownReplace(text);
        }

        private static string MarkdownReplace(string text) {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var markdown = new MarkdownSharp.Markdown(new MarkdownOptions { AutoNewLines = true });
            return markdown.Transform(text);
        }
    }
}