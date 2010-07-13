using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.Blogs.Routing;
using Orchard.Core.Common.Models;
using Orchard.ContentManagement;
using Orchard.Core.Routable.Models;

namespace Orchard.Blogs.Services {
    [UsedImplicitly]
    public class BlogService : IBlogService {
        private readonly IContentManager _contentManager;
        private readonly IBlogSlugConstraint _blogSlugConstraint;

        public BlogService(IContentManager contentManager, IBlogSlugConstraint blogSlugConstraint) {
            _contentManager = contentManager;
            _blogSlugConstraint = blogSlugConstraint;
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

        public void Create(Blog blog) {
            _contentManager.Create(blog.ContentItem);
            _blogSlugConstraint.AddSlug(blog.Slug);
        }

        public void Edit(Blog blog) {
            _blogSlugConstraint.AddSlug(blog.Slug);
        }

        public void Delete(Blog blog) {
            _contentManager.Remove(blog.ContentItem);
            _blogSlugConstraint.RemoveSlug(blog.Slug);
        }
    }
}