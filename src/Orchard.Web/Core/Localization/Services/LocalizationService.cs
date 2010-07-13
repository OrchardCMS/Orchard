using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Tasks.Scheduling;

namespace Orchard.Core.Localization.Services {
    public class LocalizationService : ILocalizationService {
        private readonly IContentManager _contentManager;
        private readonly ICultureManager _cultureManager;

        public LocalizationService(IContentManager contentManager, ICultureManager cultureManager) {
            _contentManager = contentManager;
            _cultureManager = cultureManager;
        }

        Localized ILocalizationService.GetLocalizedContentItem(IContent content, string culture) {
            return _contentManager.Query(content.ContentItem.ContentType).Join<LocalizedRecord>()
                .List()
                .Select(i => i.As<Localized>())
                .Where(l => l.MasterContentItem != null && l.MasterContentItem.ContentItem.Id == content.ContentItem.Id && string.Equals(l.Culture.Culture, culture, StringComparison.OrdinalIgnoreCase))
                .SingleOrDefault();
        }

        string ILocalizationService.GetContentCulture(IContent content) {
            return content.Is<Localized>() && content.As<Localized>().Culture != null
                       ? content.As<Localized>().Culture.Culture
                       : _cultureManager.GetSiteCulture();
        }

        IEnumerable<IContent> ILocalizationService.GetLocalizations(IContent content) {
            var localized = content.As<Localized>();

            //todo: (heskew) get scheduled content as well

            if (localized.MasterContentItem != null)
                return _contentManager.Query(VersionOptions.Latest, content.ContentItem.ContentType).Join<LocalizedRecord>()
                    .List()
                    .Select(i => i.As<Localized>())
                    .Where(l => l.Id != content.ContentItem.Id && (l.Id == localized.MasterContentItem.ContentItem.Id || l.MasterContentItem != null && l.MasterContentItem.ContentItem.Id == localized.MasterContentItem.ContentItem.Id));

            return _contentManager.Query(VersionOptions.Latest, content.ContentItem.ContentType).Join<LocalizedRecord>()
                .List()
                .Select(i => i.As<Localized>())
                .Where(l => l.MasterContentItem != null && l.MasterContentItem.ContentItem.Id == content.ContentItem.Id);
        }
    }
}