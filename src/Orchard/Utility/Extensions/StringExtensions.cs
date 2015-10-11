﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Orchard.Localization;
using System.Web;

namespace Orchard.Utility.Extensions {
    public static class StringExtensions {
        public static string CamelFriendly(this string camel) {
            if (String.IsNullOrWhiteSpace(camel))
                return "";

            var sb = new StringBuilder(camel);

            for (int i = camel.Length-1; i>0; i--) {
                if(char.IsUpper(sb[i])) {
                    sb.Insert(i, ' ');
                }
            }

            return sb.ToString();
        }

        public static string Ellipsize(this string text, int characterCount) {
            return text.Ellipsize(characterCount, "&#160;&#8230;");
        }

        public static string Ellipsize(this string text, int characterCount, string ellipsis, bool wordBoundary = false) {
            if (String.IsNullOrWhiteSpace(text))
                return "";
            
            if (characterCount < 0 || text.Length <= characterCount)
                return text;

            // search beginning of word
            var backup = characterCount;
            while (characterCount > 0 && text[characterCount-1].IsLetter()) {
                characterCount--;
            }

            // search previous word
            while (characterCount > 0 && text[characterCount - 1].IsSpace()) {
                characterCount--;
            }

            // if it was the last word, recover it, unless boundary is requested
            if(characterCount == 0 && !wordBoundary) {
                characterCount = backup;
            }

            var trimmed = text.Substring(0, characterCount);
            return trimmed + ellipsis;
        }

        public static string HtmlClassify(this string text) {
            if (String.IsNullOrWhiteSpace(text))
                return "";

            var friendlier = text.CamelFriendly();

            var result = new char[friendlier.Length];

            var cursor = 0;
            var previousIsNotLetter = false;
            for (var i = 0; i < friendlier.Length; i++) {
                char current = friendlier[i];
                if (IsLetter(current) || (Char.IsDigit(current) && cursor > 0)) {
                    if (previousIsNotLetter && i != 0 && cursor > 0) {
                        result[cursor++] = '-';    
                    }
                    
                    result[cursor++] = Char.ToLowerInvariant(current);
                    previousIsNotLetter = false;
                }
                else {
                    previousIsNotLetter = true;
                }
            }

            return new string(result, 0, cursor);
        }

        public static LocalizedString OrDefault(this string text, LocalizedString defaultValue) {
            return String.IsNullOrEmpty(text)
                ? defaultValue
                : new LocalizedString(text);
        }

        public static string RemoveTags(this string html, bool htmlDecode = false) {
            if (String.IsNullOrEmpty(html)) {
                return String.Empty;
            }

            var result = new char[html.Length];

            var cursor = 0;
            var inside = false;
            for (var i = 0; i < html.Length; i++) {
                char current = html[i];
                
                switch(current) {
                    case '<':
                        inside = true;
                        continue;
                    case '>':
                        inside = false;
                        continue;
                }

                if (!inside) {
                    result[cursor++] = current;
                }
            }

            var stringResult = new string(result, 0, cursor);

            if (htmlDecode) {
                stringResult = HttpUtility.HtmlDecode(stringResult);
            }

            return stringResult;
        }

        // not accounting for only \r (e.g. Apple OS 9 carriage return only new lines)
        public static string ReplaceNewLinesWith(this string text, string replacement) {
            return String.IsNullOrWhiteSpace(text)
                       ? String.Empty
                       : text
                             .Replace("\r\n", "\r\r")
                             .Replace("\n", String.Format(replacement, "\r\n"))
                             .Replace("\r\r", String.Format(replacement, "\r\n"));
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

        private static readonly char[] validSegmentChars = "/?#[]@\"^{}|`<>\t\r\n\f ".ToCharArray();
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

            return !segment.Any(validSegmentChars);
        }

        /// <summary>
        /// Generates a valid technical name.
        /// </summary>
        /// <remarks>
        /// Uses a white list set of chars.
        /// </remarks>
        public static string ToSafeName(this string name) {
            if (String.IsNullOrWhiteSpace(name))
                return String.Empty;

            name = RemoveDiacritics(name);
            name = name.Strip(c => 
                !c.IsLetter()
                && !Char.IsDigit(c)
                );

            name = name.Trim();

            // don't allow non A-Z chars as first letter, as they are not allowed in prefixes
            while (name.Length > 0 && !IsLetter(name[0])) {
                name = name.Substring(1);
            }

            if (name.Length > 128)
                name = name.Substring(0, 128);

            return name;
        }

        /// <summary>
        /// Generates a valid Html name.
        /// </summary>
        /// <remarks>
        /// Uses a white list set of chars.
        /// </remarks>
        public static string ToHtmlName(this string name) {
            if (String.IsNullOrWhiteSpace(name))
                return String.Empty;

            name = RemoveDiacritics(name);
            name = name.Strip(c =>
                c != '-'
                && c != '_'
                && !c.IsLetter()
                && !Char.IsDigit(c)
                );

            name = name.Trim();

            // don't allow non A-Z chars as first letter, as they are not allowed in prefixes
            while (name.Length > 0 && !IsLetter(name[0])) {
                name = name.Substring(1);
            }

            return name;
        }

        /// <summary>
        /// Whether the char is a letter between A and Z or not
        /// </summary>
        public static bool IsLetter(this char c) {
            return ('A' <= c && c <= 'Z') || ('a' <= c && c <= 'z');
        }

        public static bool IsSpace(this char c) {
            return (c == '\r' || c == '\n' || c == '\t' || c == '\f' || c == ' ');
        }

        public static string RemoveDiacritics(this string name) {
            string stFormD = name.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (char t in stFormD) {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(t);
                if (uc != UnicodeCategory.NonSpacingMark) {
                    sb.Append(t);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }

        public static string Strip(this string subject, params char[] stripped) {
            if(stripped == null || stripped.Length == 0 || String.IsNullOrEmpty(subject)) {
                return subject;
            }

            var result = new char[subject.Length];

            var cursor = 0;
            for (var i = 0; i < subject.Length; i++) {
                char current = subject[i];
                if (Array.IndexOf(stripped, current) < 0) {
                    result[cursor++] = current;
                }
            }

            return new string(result, 0, cursor);
        }

        public static string Strip(this string subject, Func<char, bool> predicate) {

            var result = new char[subject.Length];

            var cursor = 0;
            for (var i = 0; i < subject.Length; i++) {
                char current = subject[i];
                if (!predicate(current)) {
                    result[cursor++] = current;
                }
            }

            return new string(result, 0, cursor);
        }

        public static bool Any(this string subject, params char[] chars) {
            if (string.IsNullOrEmpty(subject) || chars == null || chars.Length == 0) {
                return false;
            }

            for (var i = 0; i < subject.Length; i++) {
                char current = subject[i];
                if (Array.IndexOf(chars, current) >= 0) {
                    return true;
                }
            }

            return false;
        }

        public static bool All(this string subject, params char[] chars) {
            if (string.IsNullOrEmpty(subject)) {
                return true;
            }

            if(chars == null || chars.Length == 0) {
                return false;
            }

            for (var i = 0; i < subject.Length; i++) {
                char current = subject[i];
                if (Array.IndexOf(chars, current) < 0) {
                    return false;
                }
            }

            return true;
        }

        public static string Translate(this string subject, char[] from, char[] to) {
            if (string.IsNullOrEmpty(subject)) {
                return subject;
            }

            if (from == null || to == null) {
                throw new ArgumentNullException();
            }

            if (from.Length != to.Length) {
                throw new ArgumentNullException("from", "Parameters must have the same length");
            }

            var map = new Dictionary<char, char>(from.Length);
            for (var i = 0; i < from.Length; i++) {
                map[from[i]] = to[i];
            }

            var result = new char[subject.Length];

            for (var i = 0; i < subject.Length; i++) {
                var current = subject[i];
                if (map.ContainsKey(current)) {
                    result[i] = map[current];
                }
                else {
                    result[i] = current;
                }
            }

            return new string(result);
        }

        public static string ReplaceAll(this string original, IDictionary<string, string> replacements) {
            var pattern = String.Format("{0}", String.Join("|", replacements.Keys));
            return Regex.Replace(original, pattern, match => replacements[match.Value]);
        }

        public static string ToBase64(this string value) {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }

        public static string FromBase64(this string value) {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }
    }
}
