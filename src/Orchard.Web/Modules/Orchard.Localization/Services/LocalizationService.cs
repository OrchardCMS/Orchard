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
            // Warning: Returns only the first of same culture localizations.
            return ((ILocalizationService) this).GetLocalizedContentItem(content, culture, null);
        }

        LocalizationPart ILocalizationService.GetLocalizedContentItem(IContent content, string culture, VersionOptions versionOptions) {
            var cultureRecord = _cultureManager.GetCultureByName(culture);

            if (cultureRecord == null)
                return null;

            var localized = content.As<LocalizationPart>();

            if (localized == null)
                return null;

            var query = versionOptions == null
                ? _contentManager.Query(content.ContentItem.ContentType)
                : _contentManager.Query(versionOptions, content.ContentItem.ContentType);

            // Warning: Returns only the first of same culture localizations.
            return query.Where<LocalizationPartRecord>(l =>
                (l.Id == content.ContentItem.Id || l.MasterContentItemId == content.ContentItem.Id)
                && l.CultureId == cultureRecord.Id)
                .List()
                .Select(i => i.As<LocalizationPart>())
                .FirstOrDefault();
        }

        string ILocalizationService.GetContentCulture(IContent content) {
            var localized = content.As<LocalizationPart>();
            return localized != null && localized.Culture != null
                ? localized.Culture.Culture
                : _cultureManager.GetSiteCulture();
        }

        void ILocalizationService.SetContentCulture(IContent content, string culture) {
            var localized = content.As<LocalizationPart>();
            if (localized == null)
                return;

            var cultureRecord = _cultureManager.GetCultureByName(culture);

            localized.Culture = cultureRecord;
        }

        IEnumerable<LocalizationPart> ILocalizationService.GetLocalizations(IContent content) {
            // Warning: May contain more than one localization of the same culture.
            return ((ILocalizationService) this).GetLocalizations(content, null);
        }

        IEnumerable<LocalizationPart> ILocalizationService.GetLocalizations(IContent content, VersionOptions versionOptions) {
            if (content.ContentItem.Id == 0)
                return Enumerable.Empty<LocalizationPart>();

            var localized = content.As<LocalizationPart>();

            var query = versionOptions == null
                ? _contentManager.Query(localized.ContentItem.ContentType)
                : _contentManager.Query(versionOptions, localized.ContentItem.ContentType);

            int contentItemId = localized.ContentItem.Id;

            if (localized.MasterContentItem != null) {
                int masterContentItemId = localized.MasterContentItem.ContentItem.Id;

                query = query.Where<LocalizationPartRecord>(l =>
                    l.Id != contentItemId // Exclude the content
                    && (l.Id == masterContentItemId || l.MasterContentItemId == masterContentItemId));
            }
            else {
                query = query.Where<LocalizationPartRecord>(l =>
                    l.MasterContentItemId == contentItemId);
            }

            // Warning: May contain more than one localization of the same culture.
            return query.List().Select(i => i.As<LocalizationPart>());
        }
    }
}