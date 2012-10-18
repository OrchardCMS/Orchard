using System;
using System.Collections.Generic;
using System.Text;
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

            return sb.ToString();
        }
    }
}