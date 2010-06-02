using System.Collections.Generic;
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

            if (cultureName.Equals("en-US")) {
                return text;
            }

            return string.Empty;
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
            return ParseLocalizationStream(text);
        }

        private static IDictionary<string, string> ParseLocalizationStream(string text) {
            return new Dictionary<string, string>();
        }
    }

    class CultureDictionary {
        public string CultureName { get; set; }
        public IDictionary<string, string> Translations { get; set; }
    }
}
