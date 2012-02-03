using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.Blogs.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Title.Models;

namespace Orchard.Blogs.Services {
    [UsedImplicitly]
    public class BlogService : IBlogService {
        private readonly IContentManager _contentManager;
        private readonly IBlogPathConstraint _blogPathConstraint;

        public BlogService(IContentManager contentManager, IBlogPathConstraint blogPathConstraint) {
            _contentManager = contentManager;
            _blogPathConstraint = blogPathConstraint;
        }

        public BlogPart Get(string path) {
            return _contentManager.Query<BlogPart>().List().FirstOrDefault(rr => rr.As<IAliasAspect>().Path == path);
        }

        public ContentItem Get(int id, VersionOptions versionOptions) {
            return _contentManager.Get(id, versionOptions);
        }

        public IEnumerable<BlogPart> Get() {
            return Get(VersionOptions.Published);
        }

        public IEnumerable<BlogPart> Get(VersionOptions versionOptions) {
            return _contentManager.Query<BlogPart, BlogPartRecord>(versionOptions)
                .Join<TitlePartRecord>()
                .OrderBy(br => br.Title)
                .List();
        }

        public void Delete(ContentItem blog) {
            _contentManager.Remove(blog);
            _blogPathConstraint.RemovePath(blog.As<IAliasAspect>().Path);
        }
    }
}