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
        private readonly IPublishingTaskManager _publishingTaskManager;

        public LocalizationService(IContentManager contentManager, ICultureManager cultureManager, IPublishingTaskManager publishingTaskManager) {
            _contentManager = contentManager;
            _cultureManager = cultureManager;
            _publishingTaskManager = publishingTaskManager;
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

            if (localized.MasterContentItem != null)
                return _contentManager.Query(content.ContentItem.ContentType).Join<LocalizedRecord>()
                    .List()
                    .Select(i => i.As<Localized>())
                    .Where(l => l.Id != content.ContentItem.Id && (l.Id == localized.MasterContentItem.ContentItem.Id || l.MasterContentItem != null && l.MasterContentItem.ContentItem.Id == localized.MasterContentItem.ContentItem.Id));

            return _contentManager.Query(content.ContentItem.ContentType).Join<LocalizedRecord>()
                .List()
                .Select(i => i.As<Localized>())
                .Where(l => l.MasterContentItem != null && l.MasterContentItem.ContentItem.Id == content.ContentItem.Id);
        }

        void ILocalizationService.Publish(ContentItem contentItem) {
            _publishingTaskManager.DeleteTasks(contentItem);
            _contentManager.Publish(contentItem);
        }

        void ILocalizationService.Publish(ContentItem contentItem, DateTime scheduledPublishUtc) {
            _publishingTaskManager.Publish(contentItem, scheduledPublishUtc);
        }
    }
}