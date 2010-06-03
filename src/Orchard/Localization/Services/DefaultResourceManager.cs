using System;
using System.Collections.Generic;
using System.IO;
using Orchard.FileSystems.WebSite;

namespace Orchard.Localization.Services {
    public class DefaultResourceManager : IResourceManager {
        private readonly IWebSiteFolder _webSiteFolder;
        private readonly ICultureManager _cultureManager;
        private readonly IList<CultureDictionary> _cultures;
        const string CoreLocalizationFilePathFormat = "/Core/App_Data/Localization/{0}/orchard.core.po";

        public DefaultResourceManager(ICultureManager cultureManager, IWebSiteFolder webSiteFolder) {
            _cultureManager = cultureManager;
            _webSiteFolder = webSiteFolder;
            _cultures = new List<CultureDictionary>();
        }

        public string GetLocalizedString(string scope, string text, string cultureName) {
            if (_cultures.Count == 0) {
                LoadCultures();
            }

            foreach (var culture in _cultures) {
                if (String.Equals(cultureName, culture.CultureName, StringComparison.OrdinalIgnoreCase)) {
                    string scopedKey = scope + "|" + text;
                    string genericKey = "|" + text;
                    if (culture.Translations.ContainsKey(scopedKey)) {
                        return culture.Translations[scopedKey];
                    }
                    if (culture.Translations.ContainsKey(genericKey)) {
                        return culture.Translations[genericKey];
                    }
                    return text;
                }
            }

            return text;
        }

        private void LoadCultures() {
            foreach (var culture in _cultureManager.ListCultures()) {
                _cultures.Add(new CultureDictionary {
                    CultureName = culture,
                    Translations = LoadTranslationsForCulture(culture)
                });
            }
        }

        private IDictionary<string, string> LoadTranslationsForCulture(string culture) {
            string path = string.Format(CoreLocalizationFilePathFormat, culture);
            string text = _webSiteFolder.ReadFile(path);
            if (text != null) {
                return ParseLocalizationStream(text);
            }
            return new Dictionary<string, string>();
        }

        private static IDictionary<string, string> ParseLocalizationStream(string text) {
            Dictionary<string, string> translations = new Dictionary<string, string>();
            StringReader reader = new StringReader(text);
            string poLine, id, scope;
            id = scope = String.Empty;
            while ((poLine = reader.ReadLine()) != null) {
                if (poLine.StartsWith("#:")) {
                    scope = ParseScope(poLine);
                    continue;
                }

                if (poLine.StartsWith("msgid")) {
                    id = ParseId(poLine);
                    continue;
                }

                if (poLine.StartsWith("msgstr")) {
                    string translation = ParseTranslation(poLine);
                    if (!String.IsNullOrEmpty(id)) {
                        if (!String.IsNullOrEmpty(scope)) {
                            string scopedKey = scope + "|" + id;
                            if (!translations.ContainsKey(scopedKey)) {
                                translations.Add(scopedKey, translation);
                            }
                        }
                        string genericKey = "|" + id;
                        if (!translations.ContainsKey(genericKey)) {
                            translations.Add(genericKey, translation);
                        }
                    }
                    id = scope = String.Empty;
                }

            }

            return translations;
        }

        private static string ParseTranslation(string poLine) {
            return poLine.Substring(6).Trim().Trim('"');
        }

        private static string ParseId(string poLine) {
            return poLine.Substring(5).Trim().Trim('"');
        }

        private static string ParseScope(string poLine) {
            return poLine.Substring(2).Trim().Trim('"');
        }

        class CultureDictionary {
            public string CultureName { get; set; }
            public IDictionary<string, string> Translations { get; set; }
        }
    }
}
