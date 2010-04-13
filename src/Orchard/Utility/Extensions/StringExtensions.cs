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
        public static bool IsNullOrEmptyTrimmed(this string text) {
            if (text == null) return true;
            return string.IsNullOrEmpty(text.Trim());
        }
    }
}