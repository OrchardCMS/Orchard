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

        public static bool IsValidUrlSegment(this string segment) {
            // valid isegment from rfc3987 - http://tools.ietf.org/html/rfc3987#page-8
            // the relevant bits:
            // isegment    = *ipchar
            // ipchar      = iunreserved / pct-encoded / sub-delims / ":" / "@"
            // iunreserved = ALPHA / DIGIT / "-" / "." / "_" / "~" / ucschar
            // pct-encoded = "%" HEXDIG HEXDIG
            // sub-delims  = "!" / "$" / "&" / "'" / "(" / ")" / "*" / "+" / "," / ";" / "="
            // ucschar     = %xA0-D7FF / %xF900-FDCF / %xFDF0-FFEF / %x10000-1FFFD / %x20000-2FFFD / %x30000-3FFFD / %x40000-4FFFD / %x50000-5FFFD / %x60000-6FFFD / %x70000-7FFFD / %x80000-8FFFD / %x90000-9FFFD / %xA0000-AFFFD / %xB0000-BFFFD / %xC0000-CFFFD / %xD0000-DFFFD / %xE1000-EFFFD
            // 
            // rough blacklist regex == m/^[^/?#[]@"^{}|\s`<>]+$/ (leaving off % to keep the regex simple)

            return Regex.IsMatch(segment, @"^[^/?#[\]@""^{}|`<>\s]+$");
        }
    }
}