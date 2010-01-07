using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Pages.Models;
using Orchard.Core.Common.Records;
using Orchard.Data;
using Orchard.ContentManagement;

namespace Orchard.Pages.Services {
    public class PageService : IPageService {
        private readonly IContentManager _contentManager;
        private readonly IRepository<PageRecord> _pageRepository;

        public PageService(IContentManager contentManager, IRepository<PageRecord> pageRepository) {
            _contentManager = contentManager;
            _pageRepository = pageRepository;
        }

        public IEnumerable<Page> Get() {
            return _contentManager.Query<Page, PageRecord>().List();
        }

        public Page Get(string slug) {
            return _contentManager.Query<Page, PageRecord>()
                .Join<RoutableRecord>().Where(rr => rr.Slug == slug)
                .List().FirstOrDefault();
        }

        public void Delete(Page page) {
            _pageRepository.Delete(page.Record);
        }
    }
}