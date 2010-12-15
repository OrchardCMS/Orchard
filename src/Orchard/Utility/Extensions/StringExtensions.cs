using System;
using System.Linq;
using System.Text.RegularExpressions;
using Orchard.Localization;

namespace Orchard.Utility.Extensions {
    public static class StringExtensions {
        private static readonly Regex humps = new Regex("(?:^[a-zA-Z][^A-Z]*|[A-Z][^A-Z]*)");
        public static string CamelFriendly(this string camel) {
            if (string.IsNullOrWhiteSpace(camel))
                return "";

            var matches = humps.Matches(camel).OfType<Match>().Select(m => m.Value);
            return matches.Any()
                ? matches.Aggregate((a, b) => a + " " + b).TrimStart(' ')
                : camel;
        }

        public static string Ellipsize(this string text, int characterCount) {
            return text.Ellipsize(characterCount, "&#160;&#8230;");
        }

        public static string Ellipsize(this string text, int characterCount, string ellipsis) {
            if (string.IsNullOrWhiteSpace(text))
                return "";
            
            if (characterCount < 0 || text.Length <= characterCount)
                return text;

            return Regex.Replace(text.Substring(0, characterCount + 1), @"\s+\S*$", "") + ellipsis;
        }

        public static string HtmlClassify(this string text) {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            var friendlier = text.CamelFriendly();
            return Regex.Replace(friendlier, @"[^a-zA-Z]+", m => m.Index == 0 ? "" : "-").ToLowerInvariant();
        }

        public static LocalizedString OrDefault(this string text, LocalizedString defaultValue) {
            return string.IsNullOrEmpty(text)
                ? defaultValue
                : new LocalizedString(text);
        }

        public static string RemoveTags(this string html) {
            return string.IsNullOrEmpty(html)
                ? ""
                : Regex.Replace(html, "<[^<>]*>", "", RegexOptions.Singleline);
        }

        // not accounting for only \r (e.g. Apple OS 9 carriage return only new lines)
        public static string ReplaceNewLinesWith(this string text, string replacement) {
            return string.IsNullOrWhiteSpace(text)
                ? ""
                : Regex.Replace(text, @"(\r?\n)", replacement, RegexOptions.Singleline);
        }

        public static string ToHexString(this byte[] bytes) {
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        public static byte[] ToByteArray(this string hex) {
            return Enumerable.Range(0, hex.Length).
                Where(x => 0 == x % 2).
                Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).
                ToArray();
        }
    }
}