using System;
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
            return ((ILocalizationService)this).GetLocalizedContentItem(content, culture, null);
        }

        LocalizationPart ILocalizationService.GetLocalizedContentItem(IContent content, string culture, VersionOptions versionOptions) {
            var cultureRecord = _cultureManager.GetCultureByName(culture);

            if (cultureRecord == null)
                return null;

            var localized = content.As<LocalizationPart>();

            if (localized == null)
                return null;

            var masterContent = content.ContentItem.As<LocalizationPart>().MasterContentItem != null ? content.ContentItem.As<LocalizationPart>().MasterContentItem : content;
            // Warning: Returns only the first of same culture localizations.
            return _contentManager
                .Query<LocalizationPart>(versionOptions, content.ContentItem.ContentType)
                .Where<LocalizationPartRecord>(l =>
                (l.Id == masterContent.Id || l.MasterContentItemId == masterContent.Id)
                && l.CultureId == cultureRecord.Id)
                .Slice(1)
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

            localized.Culture = _cultureManager.GetCultureByName(culture);
        }

        IEnumerable<LocalizationPart> ILocalizationService.GetLocalizations(IContent content) {
            // Warning: May contain more than one localization of the same culture.
            return ((ILocalizationService)this).GetLocalizations(content, null);
        }

        IEnumerable<LocalizationPart> ILocalizationService.GetLocalizations(IContent content, VersionOptions versionOptions) {
            if (content.ContentItem.Id == 0)
                return Enumerable.Empty<LocalizationPart>();
            var localized = content.As<LocalizationPart>();
            IContentQuery<LocalizationPart> query;
            if (content.ContentItem.TypeDefinition.Parts.Any(x => x.PartDefinition.Name == "TermPart")) { // terms translations can be contained on different TermContentType linked to taxonomies translations
                query = versionOptions == null
                 ? _contentManager.Query<LocalizationPart>()
                 : _contentManager.Query<LocalizationPart>(versionOptions);
            }
            else {
                query = versionOptions == null
                  ? _contentManager.Query<LocalizationPart>(localized.ContentItem.ContentType)
                  : _contentManager.Query<LocalizationPart>(versionOptions, localized.ContentItem.ContentType);
            }

            int contentItemId = localized.ContentItem.Id;

            if (localized.HasTranslationGroup) {
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
            return query.List().ToList();
        }

    }
}