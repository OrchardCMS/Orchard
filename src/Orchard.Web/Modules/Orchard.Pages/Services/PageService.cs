using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.Records;
using Orchard.Pages.Models;
using Orchard.Core.Common.Records;
using Orchard.ContentManagement;
using Orchard.Tasks.Scheduling;

namespace Orchard.Pages.Services {
    public class PageService : IPageService {
        private readonly IContentManager _contentManager;
        private readonly IPublishingTaskManager _publishingTaskManager;
        private readonly ISlugConstraint _slugConstraint;

        public PageService(IContentManager contentManager, IPublishingTaskManager publishingTaskManager, ISlugConstraint slugConstraint) {
            _contentManager = contentManager;
            _publishingTaskManager = publishingTaskManager;
            _slugConstraint = slugConstraint;
        }

        public IEnumerable<Page> Get() {
            return Get(PageStatus.All);
        }

        public IEnumerable<Page> Get(PageStatus status) {
            switch (status) {
                case PageStatus.All:
                    return _contentManager.Query<Page>(VersionOptions.Latest).List();
                case PageStatus.Published:
                    return _contentManager.Query<Page>(VersionOptions.Published).List();
                case PageStatus.Offline:
                    return _contentManager.Query<Page>(VersionOptions.Latest).Where<ContentPartVersionRecord>(ci => !ci.ContentItemVersionRecord.Published).List();
                default:
                    return Enumerable.Empty<Page>();
            }
        }

        public Page Get(int id) {
            return _contentManager.Get<Page>(id);
        }

        public Page Get(string slug) {
            return
                _contentManager.Query("page").Join<RoutableRecord>().Where(rr => rr.Slug == slug).List().FirstOrDefault
                    ().As<Page>();
        }

        public Page GetLatest(int id) {
            return _contentManager.Get<Page>(id, VersionOptions.Latest);
        }

        public Page GetLatest(string slug) {
            return
                _contentManager.Query(VersionOptions.Latest, "page").Join<RoutableRecord>().Where(rr => rr.Slug == slug)
                    .Slice(0, 1).FirstOrDefault().As<Page>();
        }

        public Page GetPageOrDraft(int id)  {
            return _contentManager.GetDraftRequired<Page>(id);
        }

        public Page GetPageOrDraft(string slug) {
            Page page = GetLatest(slug);
            return _contentManager.GetDraftRequired<Page>(page.Id);
        }

        public void Delete(Page page) {
            _publishingTaskManager.DeleteTasks(page.ContentItem);
            _contentManager.Remove(page.ContentItem);
        }

        public void Publish(Page page) {
            _publishingTaskManager.DeleteTasks(page.ContentItem);
            _contentManager.Publish(page.ContentItem);
            _slugConstraint.AddPublishedSlug(page.Slug);
        }

        public void Publish(Page page, DateTime scheduledPublishUtc) {
            _publishingTaskManager.Publish(page.ContentItem, scheduledPublishUtc);
        }

        public void Unpublish(Page page) {
            _contentManager.Unpublish(page.ContentItem);
            _slugConstraint.RemovePublishedSlug(page.Slug);
        }

        public DateTime? GetScheduledPublishUtc(Page page) {
            var task = _publishingTaskManager.GetPublishTask(page.ContentItem);
            return (task == null ? null : task.ScheduledUtc);
        }
    }
}