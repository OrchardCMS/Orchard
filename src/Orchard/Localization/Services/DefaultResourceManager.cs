using System;
using System.Collections.Generic;
using System.IO;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.WebSite;

namespace Orchard.Localization.Services {
    public class DefaultResourceManager : IResourceManager {
        private readonly IWebSiteFolder _webSiteFolder;
        private readonly ICultureManager _cultureManager;
        private readonly IExtensionManager _extensionManager;
        private readonly ShellSettings _shellSettings;
        private readonly IList<CultureDictionary> _cultures;
        const string CoreLocalizationFilePathFormat = "/Core/App_Data/Localization/{0}/orchard.core.po";
        const string ModulesLocalizationFilePathFormat = "/Modules/{0}/App_Data/Localization/{1}/orchard.module.po";
        const string RootLocalizationFilePathFormat = "/App_Data/Localization/{0}/orchard.root.po";
        const string TenantLocalizationFilePathFormat = "/App_Data/Sites/{0}/Localization/{1}/orchard.po";

        public DefaultResourceManager(
            ICultureManager cultureManager, 
            IWebSiteFolder webSiteFolder, 
            IExtensionManager extensionManager, 
            ShellSettings shellSettings) {
            _cultureManager = cultureManager;
            _webSiteFolder = webSiteFolder;
            _extensionManager = extensionManager;
            _shellSettings = shellSettings;
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
            IDictionary<string, string> translations = new Dictionary<string, string>();
            string corePath = string.Format(CoreLocalizationFilePathFormat, culture);
            string text = _webSiteFolder.ReadFile(corePath);
            if (text != null) {
                ParseLocalizationStream(text, translations, false);
            }

            foreach (var module in _extensionManager.AvailableExtensions()) {
                if (String.Equals(module.ExtensionType, "Module")) {
                    string modulePath = string.Format(ModulesLocalizationFilePathFormat, module.Name, culture);
                    text = _webSiteFolder.ReadFile(modulePath);
                    if (text != null) {
                        ParseLocalizationStream(text, translations, true);
                    }
                }
            }

            string rootPath = string.Format(RootLocalizationFilePathFormat, culture);
            text = _webSiteFolder.ReadFile(rootPath);
            if (text != null) {
                ParseLocalizationStream(text, translations, true);
            }

            string tenantPath = string.Format(TenantLocalizationFilePathFormat, _shellSettings.Name, culture);
            text = _webSiteFolder.ReadFile(tenantPath);
            if (text != null) {
                ParseLocalizationStream(text, translations, true);
            }

            return translations;
        }

        private static void ParseLocalizationStream(string text, IDictionary<string, string> translations, bool merge) {
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
                            else {
                                if (merge) {
                                    translations[scopedKey] = translation;
                                }
                            }
                        }
                        string genericKey = "|" + id;
                        if (!translations.ContainsKey(genericKey)) {
                            translations.Add(genericKey, translation);
                        }
                        else {
                            if (merge) {
                                translations[genericKey] = translation;
                            }
                        }
                    }
                    id = scope = String.Empty;
                }

            }
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
