using System.Collections.Generic;
using System.Globalization;
using Orchard.Caching;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.WebSite;
using Orchard.Logging;
using Orchard.Environment.Descriptor.Models;
using System.Linq;

namespace Orchard.Localization.Services {
    public class DefaultLocalizedStringManager : ILocalizedStringManager {
        private readonly IWebSiteFolder _webSiteFolder;
        private readonly IExtensionManager _extensionManager;
        private readonly ICacheManager _cacheManager;
        private readonly ILocalizationStreamParser _localizationStreamParser;
        private readonly ShellSettings _shellSettings;
        private readonly ISignals _signals;
        private readonly ShellDescriptor _shellDescriptor;

        const string CoreLocalizationFilePathFormat = "~/Core/App_Data/Localization/{0}/orchard.core.po";
        const string ModulesLocalizationFilePathFormat = "{0}/App_Data/Localization/{1}/orchard.module.po";
        const string ThemesLocalizationFilePathFormat = "{0}/App_Data/Localization/{1}/orchard.theme.po";
        const string RootLocalizationFilePathFormat = "~/App_Data/Localization/{0}/orchard.root.po";
        const string TenantLocalizationFilePathFormat = "~/App_Data/Sites/{0}/Localization/{1}/orchard.po";

        public DefaultLocalizedStringManager(
            IWebSiteFolder webSiteFolder,
            IExtensionManager extensionManager,
            ICacheManager cacheManager,
            ILocalizationStreamParser locationStreamParser,
            ShellSettings shellSettings,
            ISignals signals,
            ShellDescriptor shellDescriptor) {
            _webSiteFolder = webSiteFolder;
            _extensionManager = extensionManager;
            _cacheManager = cacheManager;
            _localizationStreamParser = locationStreamParser;
            _shellSettings = shellSettings;
            _signals = signals;
            _shellDescriptor = shellDescriptor;

            Logger = NullLogger.Instance;
        }

        ILogger Logger { get; set; }
        public bool DisableMonitoring { get; set; }

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
            return _cacheManager.Get(culture, true, ctx => {
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
                _localizationStreamParser.ParseLocalizationStream(text, translations, false);
                if (!DisableMonitoring) {
                    Logger.Debug("Monitoring virtual path \"{0}\"", corePath);
                    context.Monitor(_webSiteFolder.WhenPathChanges(corePath));
                }
            }

            foreach (var module in _extensionManager.AvailableExtensions()) {
                if (DefaultExtensionTypes.IsModule(module.ExtensionType)) {
                    string modulePath = string.Format(ModulesLocalizationFilePathFormat, module.VirtualPath, culture);
                    text = _webSiteFolder.ReadFile(modulePath);
                    if (text != null) {
                        _localizationStreamParser.ParseLocalizationStream(text, translations, true);

                        if (!DisableMonitoring) {
                            Logger.Debug("Monitoring virtual path \"{0}\"", modulePath);
                            context.Monitor(_webSiteFolder.WhenPathChanges(modulePath));
                        }
                    }
                }
            }

            foreach (var theme in _extensionManager.AvailableExtensions()) {
                if (DefaultExtensionTypes.IsTheme(theme.ExtensionType) && _shellDescriptor.Features.Any(x => x.Name == theme.Id))
                {

                    string themePath = string.Format(ThemesLocalizationFilePathFormat, theme.VirtualPath, culture);
                    text = _webSiteFolder.ReadFile(themePath);
                    if (text != null) {
                        _localizationStreamParser.ParseLocalizationStream(text, translations, true);

                        if (!DisableMonitoring) {
                            Logger.Debug("Monitoring virtual path \"{0}\"", themePath);
                            context.Monitor(_webSiteFolder.WhenPathChanges(themePath));
                        }
                    }
                }
            }

            string rootPath = string.Format(RootLocalizationFilePathFormat, culture);
            text = _webSiteFolder.ReadFile(rootPath);
            if (text != null) {
                _localizationStreamParser.ParseLocalizationStream(text, translations, true);
                if (!DisableMonitoring) {
                    Logger.Debug("Monitoring virtual path \"{0}\"", rootPath);
                    context.Monitor(_webSiteFolder.WhenPathChanges(rootPath));
                }
            }

            string tenantPath = string.Format(TenantLocalizationFilePathFormat, _shellSettings.Name, culture);
            text = _webSiteFolder.ReadFile(tenantPath);
            if (text != null) {
                _localizationStreamParser.ParseLocalizationStream(text, translations, true);
                if (!DisableMonitoring) {
                    Logger.Debug("Monitoring virtual path \"{0}\"", tenantPath);
                    context.Monitor(_webSiteFolder.WhenPathChanges(tenantPath));
                }
            }

            return translations;
        }

        class CultureDictionary {
            public string CultureName { get; set; }
            public IDictionary<string, string> Translations { get; set; }
        }
    }
}
