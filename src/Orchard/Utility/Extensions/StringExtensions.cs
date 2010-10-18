using System.Linq;
using System.Text.RegularExpressions;
using Orchard.Localization;

namespace Orchard.Utility.Extensions {
    public static class StringExtensions {
        private static readonly Regex humps = new Regex("(?:^[a-zA-Z][^A-Z]*|[A-Z][^A-Z]*)");
        public static string CamelFriendly(this string camel) {
            if (camel == null)
                return null;

            var matches = humps.Matches(camel).OfType<Match>().Select(m => m.Value);
            return matches.Any()
                ? matches.Aggregate((a, b) => a + " " + b).TrimStart(' ')
                : camel;
        }

        public static string Ellipsize(this string text, int characterCount) {
            return text.Ellipsize(characterCount, "&#160;&#8230;");
        }

        public static string Ellipsize(this string text, int characterCount, string ellipsis) {
            var cleanTailRegex = new Regex(@"\s+\S*$");

            if (string.IsNullOrEmpty(text) || characterCount < 0 || text.Length <= characterCount)
                return text;

            return cleanTailRegex.Replace(text.Substring(0, characterCount + 1), "") + ellipsis;
        }

        public static string HtmlClassify(this string text) {
            var friendlier = text.CamelFriendly();
            return Regex.Replace(friendlier, @"[^a-zA-Z]+", m => m.Index == 0 ? "" : "-").ToLowerInvariant();
        }

        public static bool IsNullOrEmptyTrimmed(this string text) {
            return text == null
                || string.IsNullOrEmpty(text.Trim());
        }

        public static LocalizedString OrDefault(this string text, LocalizedString defaultValue) {
            return string.IsNullOrEmpty(text)
                ? defaultValue
                : new LocalizedString(text);
        }

        public static string RemoveTags(this string html) {
            var tagRegex = new Regex("<[^<>]*>", RegexOptions.Singleline);
            var text = tagRegex.Replace(html, "");

            return text;
        }
    }
}