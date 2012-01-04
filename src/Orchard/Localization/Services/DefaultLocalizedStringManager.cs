using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Orchard.Caching;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.WebSite;

namespace Orchard.Localization.Services {
    public class DefaultLocalizedStringManager : ILocalizedStringManager {
        private readonly IWebSiteFolder _webSiteFolder;
        private readonly IExtensionManager _extensionManager;
        private readonly ICacheManager _cacheManager;
        private readonly ShellSettings _shellSettings;
        private readonly ISignals _signals;
        const string CoreLocalizationFilePathFormat = "~/Core/App_Data/Localization/{0}/orchard.core.po";
        const string ModulesLocalizationFilePathFormat = "~/Modules/{0}/App_Data/Localization/{1}/orchard.module.po";
        const string ThemesLocalizationFilePathFormat = "~/Themes/{0}/App_Data/Localization/{1}/orchard.theme.po";
        const string RootLocalizationFilePathFormat = "~/App_Data/Localization/{0}/orchard.root.po";
        const string TenantLocalizationFilePathFormat = "~/App_Data/Sites/{0}/Localization/{1}/orchard.po";

        public DefaultLocalizedStringManager(
            IWebSiteFolder webSiteFolder,
            IExtensionManager extensionManager,
            ICacheManager cacheManager,
            ShellSettings shellSettings,
            ISignals signals) {
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
            var culture = LoadCulture(cultureName);

            string scopedKey = (scope + "|" + text).ToLowerInvariant();
            if (culture.Translations.ContainsKey(scopedKey)) {
                return culture.Translations[scopedKey];
            }

            string genericKey = ("|" + text).ToLowerInvariant();
            if (culture.Translations.ContainsKey(genericKey)) {
                return culture.Translations[genericKey];
            }

            return GetParentTranslation(scope, text, cultureName);
        }

        private string GetParentTranslation(string scope, string text, string cultureName) {
            string scopedKey = (scope + "|" + text).ToLowerInvariant();
            string genericKey = ("|" + text).ToLowerInvariant();
            try {
                CultureInfo cultureInfo = CultureInfo.GetCultureInfo(cultureName);
                CultureInfo parentCultureInfo = cultureInfo.Parent;
                if (parentCultureInfo.IsNeutralCulture) {
                    var culture = LoadCulture(parentCultureInfo.Name);
                    if (culture.Translations.ContainsKey(scopedKey)) {
                        return culture.Translations[scopedKey];
                    }
                    if (culture.Translations.ContainsKey(genericKey)) {
                        return culture.Translations[genericKey];
                    }
                    return text;
                }
            }
            catch (CultureNotFoundException) { }

            return text;
        }

        // Loads the culture dictionary in memory and caches it.
        // Cache entry will be invalidated any time the directories hosting 
        // the .po files are modified.
        private CultureDictionary LoadCulture(string culture) {
            return _cacheManager.Get(culture, ctx => {
                ctx.Monitor(_signals.When("culturesChanged"));
                return new CultureDictionary {
                    CultureName = culture,
                    Translations = LoadTranslationsForCulture(culture, ctx)
                };
            });
        }

        // Merging occurs from multiple locations:
        // In reverse priority order: 
        // "~/Core/App_Data/Localization/<culture_name>/orchard.core.po";
        // "~/Modules/<module_name>/App_Data/Localization/<culture_name>/orchard.module.po";
        // "~/Themes/<theme_name>/App_Data/Localization/<culture_name>/orchard.theme.po";
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
                if (DefaultExtensionTypes.IsModule(module.ExtensionType)) {
                    string modulePath = string.Format(ModulesLocalizationFilePathFormat, module.Id, culture);
                    text = _webSiteFolder.ReadFile(modulePath);
                    if (text != null) {
                        ParseLocalizationStream(text, translations, true);
                        context.Monitor(_webSiteFolder.WhenPathChanges(modulePath));
                    }
                }
            }

            foreach (var theme in _extensionManager.AvailableExtensions()) {
                if (DefaultExtensionTypes.IsTheme(theme.ExtensionType)) {
                    string themePath = string.Format(ThemesLocalizationFilePathFormat, theme.Id, culture);
                    text = _webSiteFolder.ReadFile(themePath);
                    if (text != null) {
                        ParseLocalizationStream(text, translations, true);
                        context.Monitor(_webSiteFolder.WhenPathChanges(themePath));
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

        private static readonly Dictionary<char, char> _escapeTranslations = new Dictionary<char, char> {
            { 'n', '\n' },
            { 'r', '\r' },
            { 't', '\t' }
        };

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

        private static void ParseLocalizationStream(string text, IDictionary<string, string> translations, bool merge) {
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

        private static string ParseTranslation(string poLine) {
            return Unescape(poLine.Substring(6).Trim().Trim('"'));
        }

        private static string ParseId(string poLine) {
            return Unescape(poLine.Substring(5).Trim().Trim('"'));
        }

        private static string ParseScope(string poLine) {
            return Unescape(poLine.Substring(2).Trim().Trim('"'));
        }

        private static string ParseContext(string poLine) {
            return Unescape(poLine.Substring(7).Trim().Trim('"'));
        }

        class CultureDictionary {
            public string CultureName { get; set; }
            public IDictionary<string, string> Translations { get; set; }
        }
    }
}
