using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Orchard.Logging;

namespace Orchard.Localization.Services {
    
    public class LocalizationStreamParser : ILocalizationStreamParser {

        private const string HashtagScope = "#:";
        private const string MsgctxtScope = "msgctxt";
        private const string MsgidScope = "msgid";
        private const string MsgstrScope = "msgstr";

        private static readonly Dictionary<char, char> _escapeTranslations = new Dictionary<char, char> {
            { 'n', '\n' },
            { 'r', '\r' },
            { 't', '\t' }
        };

        public LocalizationStreamParser() {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; private set; }

        public void ParseLocalizationStream(string text, IDictionary<string, string> translations, bool merge) {
            var reader = new StringReader(text);
            var scopes = new List<string>();
            var id = string.Empty;
            var activeScope = string.Empty;

            string currentPoLine = reader.ReadLine() ?? "";

            do
            {
                if (currentPoLine.StartsWith(HashtagScope))
                {
                    currentPoLine = Parse(HashtagScope, currentPoLine);
                    activeScope = HashtagScope;
                }
                else if (currentPoLine.StartsWith(MsgctxtScope))
                {
                    currentPoLine = Parse(MsgctxtScope, currentPoLine);
                    activeScope = MsgctxtScope;
                }
                else if (currentPoLine.StartsWith(MsgidScope))
                {
                    currentPoLine = Parse(MsgidScope, currentPoLine);
                    activeScope = MsgidScope;
                }
                else if (currentPoLine.StartsWith(MsgstrScope))
                {
                    currentPoLine = Parse(MsgstrScope, currentPoLine);
                    activeScope = MsgstrScope;
                }

                string nextPoLine = reader.ReadLine() ?? "";

                while (nextPoLine != null && (!nextPoLine.StartsWith("#") && !nextPoLine.StartsWith(MsgctxtScope) &&
                                              !nextPoLine.StartsWith(MsgidScope) && !nextPoLine.StartsWith(MsgstrScope)))
                {
                    currentPoLine = string.Concat(currentPoLine, TrimQuote(nextPoLine));
                    nextPoLine = reader.ReadLine();
                }

                switch (activeScope)
                {
                    case HashtagScope:
                    case MsgctxtScope:
                        scopes.Add(currentPoLine);
                        break;

                    case MsgidScope:
                        id = currentPoLine;
                        break;

                    case MsgstrScope:
                        if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(currentPoLine)) {
                            if (scopes.Count == 0) {
                                scopes.Add(string.Empty);
                            }
                            foreach (var scope in scopes) {
                                var scopedKey = (scope + "|" + id).ToLowerInvariant();
                                if (!translations.ContainsKey(scopedKey)) {
                                    translations.Add(scopedKey, currentPoLine);
                                }
                                else {
                                    if (merge) {
                                        translations[scopedKey] = currentPoLine;
                                    }
                                }
                            }
                        }

                        id = string.Empty;
                        scopes = new List<string>();
                        break;
                }

                currentPoLine = nextPoLine;
                activeScope = string.Empty;
            } while (currentPoLine != null);
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

        private string TrimQuote(string str) {
            if (str.StartsWith("\"") && str.EndsWith("\"")) {
                if (str.Length == 1) {
                    // Handle corner case - string containing single quote
                    Logger.Warning("Invalid localization string detected: " + str);
                    return "";
                }

                return str.Substring(1, str.Length - 2);
            }

            return str;
        }

        private string Parse(string str, string poLine)
        {
            return Unescape(TrimQuote(poLine.Substring(str.Length).Trim()));
        }
    }
}
