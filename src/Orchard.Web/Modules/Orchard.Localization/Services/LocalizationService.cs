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


        public LocalizationPart GetLocalizedContentItem(IContent content, string culture) {
            return GetLocalizedContentItem(content, culture, null);
        }

        public LocalizationPart GetLocalizedContentItem(IContent content, string culture, VersionOptions versionOptions) {
            var cultureRecord = _cultureManager.GetCultureByName(culture);

            if (cultureRecord == null) return null;

            var localized = content.As<LocalizationPart>();

            if (localized == null) return null;

            if (localized?.Culture.Culture == culture) return localized;

            // Warning: Returns only the first of same culture localizations.
            return GetLocalizationsQuery(localized, versionOptions)
                .Where<LocalizationPartRecord>(l => l.CultureId == cultureRecord.Id)
                .Slice(1)
                .FirstOrDefault();
        }

        public string GetContentCulture(IContent content) {
            var localized = content.As<LocalizationPart>();

            return localized?.Culture == null ? _cultureManager.GetSiteCulture() : localized.Culture.Culture;
        }

        public void SetContentCulture(IContent content, string culture) {
            var localized = content.As<LocalizationPart>();

            if (localized == null) return;

            localized.Culture = _cultureManager.GetCultureByName(culture);
        }

        public IEnumerable<LocalizationPart> GetLocalizations(IContent content) {
            return GetLocalizations(content, null);
        }

        public IEnumerable<LocalizationPart> GetLocalizations(IContent content, VersionOptions versionOptions) {
            if (content.ContentItem.Id == 0) return Enumerable.Empty<LocalizationPart>();

            var localized = content.As<LocalizationPart>();

            return GetLocalizationsQuery(localized, versionOptions)
                .Where<LocalizationPartRecord>(l => l.Id != localized.Id) // Exclude the current content.
                .List();
        }


        private IContentQuery<LocalizationPart> GetLocalizationsQuery(LocalizationPart localizationPart, VersionOptions versionOptions) {
            var masterId = localizationPart.HasTranslationGroup ?
                localizationPart.Record.MasterContentItemId : localizationPart.Id;

            var query = versionOptions == null ?
                _contentManager.Query<LocalizationPart>() : _contentManager.Query<LocalizationPart>(versionOptions);

            // Warning: May contain more than one localization of the same culture.
            return query.Where<LocalizationPartRecord>(l => l.Id == masterId || l.MasterContentItemId == masterId);
        }
    }
}
