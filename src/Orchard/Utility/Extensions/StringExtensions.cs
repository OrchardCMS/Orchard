using System;
using System.Linq;
using System.Text;
using Orchard.Localization;

namespace Orchard.Utility.Extensions {
    public static class StringExtensions {
        public static string CamelFriendly(this string camel) {
            if (string.IsNullOrWhiteSpace(camel))
                return "";

            var sb = new StringBuilder();
            foreach (var character in camel) {
                if (char.IsUpper(character) && sb.Length > 0)
                    sb.Append(" ");
                sb.Append(character);
            }

            return sb.ToString();
        }

        public static string Ellipsize(this string text, int characterCount) {
            return text.Ellipsize(characterCount, "&#160;&#8230;");
        }

        public static string Ellipsize(this string text, int characterCount, string ellipsis) {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            if (characterCount < 0 || text.Length <= characterCount)
                return text;

            var truncated = text.Substring(0, characterCount + 1);
            var lastSpace = truncated.LastIndexOfAny(new[] { ' ', '\t', '\r', '\n' });

            return (lastSpace > 0
                        ? truncated.Substring(0, lastSpace)
                        : truncated)
                   + ellipsis;
        }

        public static string HtmlClassify(this string text) {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            var friendlier = text.CamelFriendly();

            var sb = new StringBuilder();
            foreach (var character in friendlier) {
                if (!(char.IsLetter(character) || char.IsNumber(character))) {
                    if (sb.Length > 1 && sb[sb.Length - 1] != '-')
                        sb.Append("-");
                }
                else {
                    sb.Append(char.ToLowerInvariant(character));
                }
            }

            return sb.ToString();
        }

        public static LocalizedString OrDefault(this string text, LocalizedString defaultValue) {
            return string.IsNullOrEmpty(text)
                ? defaultValue
                : new LocalizedString(text);
        }

        public static string RemoveTags(this string html) {
            if (string.IsNullOrWhiteSpace(html))
                return "";

            var imInATag = false;
            var sb = new StringBuilder();
            foreach (var character in html) {
                if (character == '<')
                    imInATag = true;

                if (!imInATag)
                    sb.Append(character);

                if (imInATag && character == '>')
                    imInATag = false;
            }

            return sb.ToString();
        }
    }
}