using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Orchard.Services;

namespace Orchard.Core.Common.Services {
    public class BbcodeFilter : IHtmlFilter {
        public string ProcessContent(string text, string flavor) {
            return BbcodeReplace(text);
        }

        // Can be moved somewhere else once we have IoC enabled body text filters.
        private static string BbcodeReplace(string text) {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var urlRegex = new Regex(@"\[url\]([^\]]+)\[\/url\]");
            var urlRegexWithLink = new Regex(@"\[url=([^\]]+)\]([^\]]+)\[\/url\]");
            var imgRegex = new Regex(@"\[img\]([^\]]+)\[\/img\]");

            text = urlRegex.Replace(text, "<a href=\"$1\">$1</a>");
            text = urlRegexWithLink.Replace(text, "<a href=\"$1\">$2</a>");

            var matches = imgRegex.Matches(text).OfType<Match>().OrderByDescending(m => m.Groups[0].Index);
            foreach(var match in matches) {
                var index = match.Groups[0].Index;
                var length = match.Groups[0].Length;
                var url = match.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(url)) {
                    if (url[0]=='~')
                        url = VirtualPathUtility.ToAbsolute(url);
                    text =
                        text.Substring(0, index) +
                        string.Format("<img src=\"{0}\" />", url) +
                        text.Substring(index + length);
                }
            }

            text = imgRegex.Replace(text, "<img src=\"$1\" />");

            return text;
        }
    }
}