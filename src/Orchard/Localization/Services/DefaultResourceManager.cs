using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Orchard.Caching;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.WebSite;

namespace Orchard.Localization.Services {
    public class DefaultResourceManager : IResourceManager {
        private readonly IWebSiteFolder _webSiteFolder;
        private readonly ICultureManager _cultureManager;
        private readonly IExtensionManager _extensionManager;
        private readonly ICacheManager _cacheManager;
        private readonly ShellSettings _shellSettings;
        private readonly ISignals _signals;
        const string CoreLocalizationFilePathFormat = "~/Core/App_Data/Localization/{0}/orchard.core.po";
        const string ModulesLocalizationFilePathFormat = "~/Modules/{0}/App_Data/Localization/{1}/orchard.module.po";
        const string RootLocalizationFilePathFormat = "~/App_Data/Localization/{0}/orchard.root.po";
        const string TenantLocalizationFilePathFormat = "~/App_Data/Sites/{0}/Localization/{1}/orchard.po";

        public DefaultResourceManager(
            ICultureManager cultureManager, 
            IWebSiteFolder webSiteFolder, 
            IExtensionManager extensionManager,
            ICacheManager cacheManager,
            ShellSettings shellSettings,
            ISignals signals) {
            _cultureManager = cultureManager;
            _webSiteFolder = webSiteFolder;
            _extensionManager = extensionManager;
            _cacheManager = cacheManager;
            _shellSettings = shellSettings;
            _signals = signals;
        }

        // This will translate a string into a string in the target cultureName.
        // The scope portion is optional, it amounts to the location of the file containing 
        // the string in case it lives in a view, or the namespace name if the string lives in a binary.
        // If the culture doesn't have a translation for the string, it will fallback to the 
        // parent culture as defined in the .net culture hierarchy. e.g. fr-FR will fallback to fr.
        // In case it's not found anywhere, the text is returned as is.
        public string GetLocalizedString(string scope, string text, string cultureName) {
            var cultures = LoadCultures();

            foreach (var culture in cultures) {
                if (String.Equals(cultureName, culture.CultureName, StringComparison.OrdinalIgnoreCase)) {
                    string scopedKey = scope + "|" + text;
                    string genericKey = "|" + text;
                    if (culture.Translations.ContainsKey(scopedKey)) {
                        return culture.Translations[scopedKey];
                    }
                    if (culture.Translations.ContainsKey(genericKey)) {
                        return culture.Translations[genericKey];
                    }

                    return GetParentTranslation(scope, text, cultureName, cultures);
                }
            }

            return text;
        }

        private static string GetParentTranslation(string scope, string text, string cultureName, IEnumerable<CultureDictionary> cultures) {
            string scopedKey = scope + "|" + text;
            string genericKey = "|" + text;
            try {
                CultureInfo cultureInfo = CultureInfo.GetCultureInfo(cultureName);
                CultureInfo parentCultureInfo = cultureInfo.Parent;
                if (parentCultureInfo.IsNeutralCulture) {
                    foreach (var culture in cultures) {
                        if (String.Equals(parentCultureInfo.Name, culture.CultureName, StringComparison.OrdinalIgnoreCase)) {
                            if (culture.Translations.ContainsKey(scopedKey)) {
                                return culture.Translations[scopedKey];
                            }
                            if (culture.Translations.ContainsKey(genericKey)) {
                                return culture.Translations[genericKey];
                            }
                            break;
                        }
                    }
                }
            }
            catch (CultureNotFoundException) { }

            return text;
        }

        // Loads the culture dictionaries in memory and caches them.
        // Cache entry will be invalidated any time the directories hosting 
        // the .po files are modified.
        private IEnumerable<CultureDictionary> LoadCultures() {
            return _cacheManager.Get("cultures", ctx => {
                var cultures = new List<CultureDictionary>();
                foreach (var culture in _cultureManager.ListCultures()) {
                    cultures.Add(new CultureDictionary {
                        CultureName = culture,
                        Translations = LoadTranslationsForCulture(culture, ctx)
                    });
                }
                ctx.Monitor(_signals.When("culturesChanged"));
                return cultures;
            });

        }

        // Merging occurs from multiple locations:
        // In reverse priority order: 
        // "~/Core/App_Data/Localization/<culture_name>/orchard.core.po";
        // "~/Modules/<module_name>/App_Data/Localization/<culture_name>/orchard.module.po";
        // "~/App_Data/Localization/<culture_name>/orchard.root.po";
        // "~/App_Data/Sites/<tenant_name>/Localization/<culture_name>/orchard.po";
        // The dictionary entries from po files that live in higher priority locations will
        // override the ones from lower priority locations during loading of dictionaries.

        // TODO: Add culture name in the po file name to facilitate usage.
        private IDictionary<string, string> LoadTranslationsForCulture(string culture, AcquireContext<string> context) {
            IDictionary<string, string> translations = new Dictionary<string, string>();
            string corePath = string.Format(CoreLocalizationFilePathFormat, culture);
            string text = _webSiteFolder.ReadFile(corePath);
            if (text != null) {
                ParseLocalizationStream(text, translations, false);
                context.Monitor(_webSiteFolder.WhenPathChanges(corePath));
            }

            foreach (var module in _extensionManager.AvailableExtensions()) {
                if (String.Equals(module.ExtensionType, "Module")) {
                    string modulePath = string.Format(ModulesLocalizationFilePathFormat, module.Name, culture);
                    text = _webSiteFolder.ReadFile(modulePath);
                    if (text != null) {
                        ParseLocalizationStream(text, translations, true);
                        context.Monitor(_webSiteFolder.WhenPathChanges(modulePath));
                    }
                }
            }

            string rootPath = string.Format(RootLocalizationFilePathFormat, culture);
            text = _webSiteFolder.ReadFile(rootPath);
            if (text != null) {
                ParseLocalizationStream(text, translations, true);
                context.Monitor(_webSiteFolder.WhenPathChanges(rootPath));
            }

            string tenantPath = string.Format(TenantLocalizationFilePathFormat, _shellSettings.Name, culture);
            text = _webSiteFolder.ReadFile(tenantPath);
            if (text != null) {
                ParseLocalizationStream(text, translations, true);
                context.Monitor(_webSiteFolder.WhenPathChanges(tenantPath));
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
