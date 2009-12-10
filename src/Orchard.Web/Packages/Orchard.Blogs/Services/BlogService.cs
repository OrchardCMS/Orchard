using System.Collections.Generic;
using System.Linq;
using Orchard.Blogs.Models;
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
            return _contentManager.Query<Blog, BlogRecord>()
                .Join<RoutableRecord>().Where(rr => rr.Slug == slug)
                .List().FirstOrDefault();
        }

        public IEnumerable<Blog> Get() {
            return _contentManager.Query<Blog, BlogRecord>()
                .Join<RoutableRecord>()
                .OrderBy(br => br.Title)
                .List();
        }

        public void Delete(Blog blog) {
            _blogRepository.Delete(blog.Record);
        }
    }
}