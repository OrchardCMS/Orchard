using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            // optimize code path if nothing to do
            if (!text.Contains("[url]") && !text.Contains("[img]") && !text.Contains("[url=")) {
                return text;
            }

            var sb = new StringBuilder(text);

            var index = -1;
            var allIndexes = new List<int>();

            // process all [url]
            while (-1 != (index = text.IndexOf("[url]", index + 1, StringComparison.Ordinal))) {
                allIndexes.Add(index);
            }

            foreach(var start in allIndexes) {
                var end = text.IndexOf("[/url]", start, StringComparison.Ordinal);
                var url = text.Substring(start + 5 , end - start - 5);

                // substitue [url] by <a>
                sb.Remove(start, end - start + 6);
                sb.Insert(start, String.Format("<a href=\"{0}\">{0}</a>", url));
            }

            // [url={url}]
            index = -1;
            allIndexes.Clear();

            while (-1 != (index = text.IndexOf("[url=", index + 1, StringComparison.Ordinal))) {
                allIndexes.Add(index);
            }

            foreach (var start in allIndexes) {
                var urlEnd = text.IndexOf("]", start, StringComparison.Ordinal);
                var end = text.IndexOf("[/url]", start, StringComparison.Ordinal);
                var url = text.Substring(start + 5, urlEnd - start - 5);
                var title = text.Substring(urlEnd + 1, end - urlEnd - 1);

                // substitue [url] by <a>
                sb.Remove(start, end - start + 6);
                sb.Insert(start, String.Format("<a href=\"{0}\">{1}</a>", url, title));
            }

            // [img]
            index = -1;
            allIndexes.Clear();

            while (-1 != (index = text.IndexOf("[img]", index + 1, StringComparison.Ordinal))) {
                allIndexes.Add(index);
            }

            foreach (var start in allIndexes) {
                var end = text.IndexOf("[/img]", start, StringComparison.Ordinal);
                var url = text.Substring(start + 5, end - start - 5);

                // substitue [url] by <a>
                sb.Remove(start, end - start + 6);

                if (!string.IsNullOrEmpty(url)) {
                    if (url[0] == '~')
                        url = VirtualPathUtility.ToAbsolute(url);
                }

                sb.Insert(start, String.Format("<img src=\"{0}\" />", url));
            }

            //var urlRegex = new Regex(@"\[url\]([^\]]+)\[\/url\]");
            //var urlRegexWithLink = new Regex(@"\[url=([^\]]+)\]([^\]]+)\[\/url\]");
            //var imgRegex = new Regex(@"\[img\]([^\]]+)\[\/img\]");

            //text = urlRegex.Replace(text, "<a href=\"$1\">$1</a>");
            //text = urlRegexWithLink.Replace(text, "<a href=\"$1\">$2</a>");

            //var matches = imgRegex.Matches(text).OfType<Match>().OrderByDescending(m => m.Groups[0].Index);
            //foreach(var match in matches) {
            //    var index2 = match.Groups[0].Index;
            //    var length = match.Groups[0].Length;
            //    var url = match.Groups[1].Value.Trim();
            //    if (!string.IsNullOrEmpty(url)) {
            //        if (url[0]=='~')
            //            url = VirtualPathUtility.ToAbsolute(url);
            //        text =
            //            text.Substring(0, index2) +
            //            string.Format("<img src=\"{0}\" />", url) +
            //            text.Substring(index2 + length);
            //    }
            //}

            //text = imgRegex.Replace(text, "<img src=\"$1\" />");

            return sb.ToString();
        }
    }
}