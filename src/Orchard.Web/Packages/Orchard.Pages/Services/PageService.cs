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
            switch (status) {
                case PageStatus.All:
                    return _contentManager.Query<Page, PageRecord>(VersionOptions.Latest).List();
                case PageStatus.Published:
                    return _contentManager.Query<Page, PageRecord>(VersionOptions.Published).List();
                case PageStatus.Offline:
                    IEnumerable<Page> allPages = _contentManager.Query<Page, PageRecord>(VersionOptions.Latest).List();
                    List<Page> offlinePages = new List<Page>();
                    foreach (var page in allPages) {
                        if (page.ContentItem.VersionRecord.Published == false) {
                            offlinePages.Add(page);
                        }
                    }
                    return offlinePages;
                default:
                    return new List<Page>();
            }
        }

        public Page Get(string slug) {
            return _contentManager.Query<Page, PageRecord>()
                .Join<RoutableRecord>().Where(rr => rr.Slug == slug)
                .List().FirstOrDefault();
        }

        public Page GetLatest(int id) {
            return _contentManager.Get<Page>(id, VersionOptions.Latest);
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