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
            return _contentManager.Query<Page, PageRecord>(VersionOptions.Latest).List();
        }

        public Page Get(string slug) {
            return _contentManager.Query<Page, PageRecord>()
                .Join<RoutableRecord>().Where(rr => rr.Slug == slug)
                .List().FirstOrDefault();
        }

        public Page GetPageOrDraft(string slug) {
            Page page = _contentManager.Query<Page, PageRecord>(VersionOptions.Latest)
                .Join<RoutableRecord>().Where(rr => rr.Slug == slug)
                .List().FirstOrDefault();
            return _contentManager.GetDraftRequired<Page>(page.Id);
        }

        public Page New() {
            return _contentManager.New<Page>("page");
        }

        public Page Create(bool publishNow) {
            return _contentManager.Create<Page>("page", publishNow ? VersionOptions.Published : VersionOptions.Draft);
        }

        public void Delete(Page page) {
            _contentManager.Remove(page.ContentItem);
        }

        public void Publish(Page page) {
            _contentManager.Publish(page.ContentItem);
        }
    }
}