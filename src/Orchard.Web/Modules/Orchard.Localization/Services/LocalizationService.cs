using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Localization.Models;

namespace Orchard.Localization.Services {
    public class LocalizationService : ILocalizationService {
        private readonly IContentManager _contentManager;
        private readonly ICultureManager _cultureManager;

        public LocalizationService(IContentManager contentManager, ICultureManager cultureManager) {
            _contentManager = contentManager;
            _cultureManager = cultureManager;
        }

        LocalizationPart ILocalizationService.GetLocalizedContentItem(IContent content, string culture) {
            var cultureRecord = _cultureManager.GetCultureByName(culture);

            if (cultureRecord == null)
                return null;

            return _contentManager.Query(content.ContentItem.ContentType)
                .Where<LocalizationPartRecord>(l => l.MasterContentItemId == content.ContentItem.Id && l.CultureId == cultureRecord.Id)
                .List()
                .Select(i => i.As<LocalizationPart>())
                .SingleOrDefault();
        }

        string ILocalizationService.GetContentCulture(IContent content) {
            var localized = content.As<LocalizationPart>();
            return localized != null && localized.Culture != null
                       ? localized.Culture.Culture
                       : _cultureManager.GetSiteCulture();
        }

        void ILocalizationService.SetContentCulture(IContent content, string culture) {
            var localized = content.As<LocalizationPart>();
            if (localized == null || localized.MasterContentItem == null)
                return;

            localized.Culture = _cultureManager.GetCultureByName(culture);
        }

        IEnumerable<LocalizationPart> ILocalizationService.GetLocalizations(IContent content, VersionOptions versionOptions) {
            var localized = content.As<LocalizationPart>();

            if (localized.MasterContentItem != null)
                return _contentManager.Query(versionOptions, localized.ContentItem.ContentType)
                    .Where<LocalizationPartRecord>(l =>
                        l.Id != localized.ContentItem.Id
                        && (l.Id == localized.MasterContentItem.ContentItem.Id
                            || l.MasterContentItemId == localized.MasterContentItem.ContentItem.Id))
                    .List()
                    .Select(i => i.As<LocalizationPart>());

            return _contentManager.Query(versionOptions, localized.ContentItem.ContentType)
                .Where<LocalizationPartRecord>(l => l.MasterContentItemId == localized.ContentItem.Id)
                .List()
                .Select(i => i.As<LocalizationPart>());
        }
    }
}