using System.Collections.Generic;
using System.Linq;
using Orchard.Blogs.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Records;
using Orchard.Data;
using Orchard.Models;

namespace Orchard.Blogs.Services {
    public class BlogService : IBlogService {
        private readonly IContentManager _contentManager;
        private readonly IRepository<BlogRecord> _blogRepository;
        private readonly IRepository<RoutableRecord> _routableRepository;

        public BlogService(IContentManager contentManager, IRepository<BlogRecord> blogRepository, IRepository<RoutableRecord> routableRepository) {
            _contentManager = contentManager;
            _blogRepository = blogRepository;
            _routableRepository = routableRepository;
        }

        public Blog Get(string slug) {
            RoutableRecord record = _routableRepository.Get(r => r.ContentItemRecord.ContentType.Name == "blog" && r.Slug == slug);

            return record != null ?_contentManager.Get<Blog>(record.Id) : null;
        }

        public IEnumerable<Blog> Get() {
            IEnumerable<RoutableRecord> records = _routableRepository.Fetch(rr => rr.ContentItemRecord.ContentType.Name == "blog", rr => rr.Asc(rr2 => rr2.Title));

            return records.Select(rr => _contentManager.Get<Blog>(rr.Id));
        }

        public void Delete(Blog blog) {
            _blogRepository.Delete(blog.Record);
        }
    }
}