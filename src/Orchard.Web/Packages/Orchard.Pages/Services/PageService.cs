using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Pages.Models;
using Orchard.Core.Common.Records;
using Orchard.ContentManagement;

namespace Orchard.Pages.Services {
    public class PageService : IPageService {
        private readonly IContentManager _contentManager;

        public PageService(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public IEnumerable<Page> Get() {
            return Get(PageStatus.All);
        }

        public IEnumerable<Page> Get(PageStatus status) {
            IEnumerable<ContentItem> contentItems;

            switch (status) {
                case PageStatus.All:
                    contentItems = _contentManager.Query(VersionOptions.Latest, "page").List();
                    break;
                case PageStatus.Published:
                    contentItems = _contentManager.Query(VersionOptions.Published, "page").List();
                    break;
                case PageStatus.Offline:
                    contentItems = _contentManager.Query(VersionOptions.Latest, "page").List().Where(ci => !ci.VersionRecord.Published);
                    break;
                default:
                    contentItems = new List<Page>().Cast<ContentItem>();
                    break;
            }

            return contentItems.Select(ci => ci.As<Page>());
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
            _contentManager.Remove(page.ContentItem);
        }

        public void Publish(Page page) {
            _contentManager.Publish(page.ContentItem);
            //TODO: (erikpo) Not sure if this is needed or not
            page.Published = DateTime.UtcNow;
        }

        public void Publish(Page page, DateTime publishDate) {
            //TODO: (erikpo) This logic should move out of blogs and pages and into content manager
            if (page.Published != null && page.Published.Value >= DateTime.UtcNow)
                _contentManager.Unpublish(page.ContentItem);
            page.Published = publishDate;
        }

        public void Unpublish(Page page) {
            _contentManager.Unpublish(page.ContentItem);
            //TODO: (erikpo) Not sure if this is needed or not
            page.Published = null;
        }
    }
}