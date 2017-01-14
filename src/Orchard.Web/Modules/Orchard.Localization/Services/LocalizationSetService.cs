using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Localization.Models;

namespace Orchard.Localization.Services {
    public class LocalizationSetService : ILocalizationSetService {
        private readonly IContentManager _contentManager;
        private readonly ICultureManager _cultureManager;

        public LocalizationSetService(IContentManager contentManager, ICultureManager cultureManager) {
            _contentManager = contentManager;
            _cultureManager = cultureManager;
        }

        public string GetContentCulture(IContent content) {
            var localized = content.As<LocalizationPart>();
            return localized != null && localized.Culture != null
                ? localized.Culture.Culture
                : _cultureManager.GetSiteCulture();
        }

        public void SetContentCulture(IContent content, string culture) {
            var localized = content.As<LocalizationPart>();
            if (localized == null)
                return;

            localized.Culture = _cultureManager.GetCultureByName(culture);
        }

        public IEnumerable<LocalizationPart> GetLocalizations(IContent content) {
            // Warning: May contain more than one localization of the same culture.
            return GetLocalizations(content, null);
        }

        public  IEnumerable<LocalizationPart> GetLocalizations(IContent content, VersionOptions versionOptions) {
            if (content.ContentItem.Id == 0)
                return Enumerable.Empty<LocalizationPart>();

            var localized = content.As<LocalizationPart>();
            if (localized == null)
                return Enumerable.Empty<LocalizationPart>();

            return GetLocalizationSet(localized.LocalizationSetId, localized.ContentItem.ContentType, versionOptions).Except(new LocalizationPart[] { localized });
        }

        public IEnumerable<LocalizationPart> GetLocalizationSet(int localizationSetId, string contentType, VersionOptions versionOptions) {
            if (localizationSetId <= 0)
                return Enumerable.Empty<LocalizationPart>();

            var query = _contentManager.Query().ForPart<LocalizationPart>();
            if (!string.IsNullOrWhiteSpace(contentType))
                query = query.ForType(contentType);
            if (versionOptions != null)
                query = query.ForVersion(versionOptions);

            query = query.Where<LocalizationPartRecord>(lpr => lpr.LocalizationSetId == localizationSetId);
            return query.List().ToList();
        }

        public LocalizationPart GetLocalizedContentItem(IContent content, string culture) {
            // Warning: Returns only the first of same culture localizations.
            return ((ILocalizationService)this).GetLocalizedContentItem(content, culture, null);
        }

        public LocalizationPart GetLocalizedContentItem(IContent content, string culture, VersionOptions versionOptions) {
            var cultureRecord = _cultureManager.GetCultureByName(culture);

            if (cultureRecord == null)
                return null;

            var localized = content.As<LocalizationPart>();

            if (localized == null)
                return null;

            var query = versionOptions == null
                ? _contentManager.Query<LocalizationPart>(localized.ContentItem.ContentType)
                : _contentManager.Query<LocalizationPart>(versionOptions, localized.ContentItem.ContentType);
            // Warning: Returns only the first of same culture localizations.
            return query.Where<LocalizationPartRecord>(lpr => lpr.LocalizationSetId == localized.LocalizationSetId && lpr.CultureId == cultureRecord.Id).Slice(1).FirstOrDefault();
        }
    }
}