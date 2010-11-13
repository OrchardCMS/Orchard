using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.Blogs.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
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

        public BlogPart Get(string path) {
            return _contentManager.Query<BlogPart, BlogPartRecord>()
                .Join<RoutePartRecord>().Where(rr => rr.Path == path)
                .List().FirstOrDefault();
        }

        public IEnumerable<BlogPart> Get() {
            return _contentManager.Query<BlogPart, BlogPartRecord>()
                .Join<RoutePartRecord>()
                .OrderBy(br => br.Title)
                .List();
        }

        public void Delete(BlogPart blogPart) {
            _contentManager.Remove(blogPart.ContentItem);
            _blogSlugConstraint.RemoveSlug(blogPart.As<IRoutableAspect>().Path);
        }
    }
}