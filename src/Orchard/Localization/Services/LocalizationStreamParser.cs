using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Orchard.Localization.Services {
    
    public class LocalizationStreamParser : ILocalizationStreamParser {

        private static readonly Dictionary<char, char> _escapeTranslations = new Dictionary<char, char> {
            { 'n', '\n' },
            { 'r', '\r' },
            { 't', '\t' }
        };

        public void ParseLocalizationStream(string text, IDictionary<string, string> translations, bool merge) {
            StringReader reader = new StringReader(text);
            string poLine, id, scope;
            id = scope = String.Empty;
            while ((poLine = reader.ReadLine()) != null) {
                if (poLine.StartsWith("#:")) {
                    scope = ParseScope(poLine);
                    continue;
                }

                if (poLine.StartsWith("msgctxt")) {
                    scope = ParseContext(poLine);
                    continue;
                }

                if (poLine.StartsWith("msgid")) {
                    id = ParseId(poLine);
                    continue;
                }

                if (poLine.StartsWith("msgstr")) {
                    string translation = ParseTranslation(poLine);
                    // ignore incomplete localizations (empty msgid or msgstr)
                    if (!String.IsNullOrWhiteSpace(id) && !String.IsNullOrWhiteSpace(translation)) {
                        string scopedKey = (scope + "|" + id).ToLowerInvariant();
                        if (!translations.ContainsKey(scopedKey)) {
                            translations.Add(scopedKey, translation);
                        }
                        else {
                            if (merge) {
                                translations[scopedKey] = translation;
                            }
                        }
                    }
                    id = scope = String.Empty;
                }

            }
        }

        private static string Unescape(string str) {
            StringBuilder sb = null;
            bool escaped = false;
            for (var i = 0; i < str.Length; i++) {
                var c = str[i];
                if (escaped) {
                    if (sb == null) {
                        sb = new StringBuilder(str.Length);
                        if (i > 1) {
                            sb.Append(str.Substring(0, i - 1));
                        }
                    }
                    char unescaped;
                    if (_escapeTranslations.TryGetValue(c, out unescaped)) {
                        sb.Append(unescaped);
                    }
                    else {
                        // General rule: \x ==> x
                        sb.Append(c);
                    }
                    escaped = false;
                }
                else {
                    if (c == '\\') {
                        escaped = true;
                    }
                    else if (sb != null) {
                        sb.Append(c);
                    }
                }
            }
            return sb == null ? str : sb.ToString();
        }

        private static string TrimQuote(string str) {
            if (str.StartsWith("\"") && str.EndsWith("\"")) {
                return str.Substring(1, str.Length - 2);
            }

            return str;
        }

        private static string ParseTranslation(string poLine) {
            return Unescape(TrimQuote(poLine.Substring(6).Trim()));
        }

        private static string ParseId(string poLine) {
            return Unescape(TrimQuote(poLine.Substring(5).Trim()));
        }

        private static string ParseScope(string poLine) {
            return Unescape(TrimQuote(poLine.Substring(2).Trim()));
        }

        private static string ParseContext(string poLine) {
            return Unescape(TrimQuote(poLine.Substring(7).Trim()));
        }
    }
}