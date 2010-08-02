using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Localization.Models;
using Orchard.Localization.Services;

namespace Orchard.Core.Localization.Services {
    public class LocalizationService : ILocalizationService {
        private readonly IContentManager _contentManager;
        private readonly ICultureManager _cultureManager;

        public LocalizationService(IContentManager contentManager, ICultureManager cultureManager) {
            _contentManager = contentManager;
            _cultureManager = cultureManager;
        }

        LocalizationPart ILocalizationService.GetLocalizedContentItem(IContent content, string culture) {
            return _contentManager.Query(content.ContentItem.ContentType).Join<LocalizationPartRecord>()
                .List()
                .Select(i => i.As<LocalizationPart>())
                .Where(l => l.MasterContentItem != null && l.MasterContentItem.ContentItem.Id == content.ContentItem.Id && string.Equals(l.Culture.Culture, culture, StringComparison.OrdinalIgnoreCase))
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

        IEnumerable<LocalizationPart> ILocalizationService.GetLocalizations(IContent content) {
            var localized = content.As<LocalizationPart>();

            //todo: (heskew) get scheduled content as well

            if (localized.MasterContentItem != null)
                return _contentManager.Query(VersionOptions.Latest, localized.ContentItem.ContentType).Join<LocalizationPartRecord>()
                    .List()
                    .Select(i => i.As<LocalizationPart>())
                    .Where(l => l.Id != localized.ContentItem.Id
                        && (l.Id == localized.MasterContentItem.ContentItem.Id
                            || l.MasterContentItem != null && l.MasterContentItem.ContentItem.Id == localized.MasterContentItem.ContentItem.Id));

            return _contentManager.Query(VersionOptions.Latest, localized.ContentItem.ContentType).Join<LocalizationPartRecord>()
                .List()
                .Select(i => i.As<LocalizationPart>())
                .Where(l => l.MasterContentItem != null && l.MasterContentItem.ContentItem.Id == localized.ContentItem.Id);
        }
    }
}