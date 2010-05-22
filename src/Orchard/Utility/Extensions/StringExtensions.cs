using System.Text.RegularExpressions;

namespace Orchard.Utility.Extensions {
    public static class StringExtensions {
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
            return Regex.Replace(text, @"[^a-zA-Z]+", m => m.Index == 0 ? "" : "-").ToLowerInvariant();
        }

        public static bool IsNullOrEmptyTrimmed(this string text) {
            return text == null
                || string.IsNullOrEmpty(text.Trim());
        }

        public static string OrDefault(this string text, string defaultValue) {
            return string.IsNullOrEmpty(text)
                ? defaultValue
                : text;
        }
    }
}