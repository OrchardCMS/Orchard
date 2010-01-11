using System.Collections.Generic;
using System.Linq;
using Orchard.Blogs.Models;
using Orchard.Core.Common.Records;
using Orchard.ContentManagement;

namespace Orchard.Blogs.Services {
    public class BlogService : IBlogService {
        private readonly IContentManager _contentManager;

        public BlogService(IContentManager contentManager) {
            _contentManager = contentManager;
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
            _contentManager.Remove(blog.ContentItem);
        }
    }
}