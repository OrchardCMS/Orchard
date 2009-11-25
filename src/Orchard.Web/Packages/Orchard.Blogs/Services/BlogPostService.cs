using System.Collections.Generic;
using System.Linq;
using Orchard.Blogs.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Records;
using Orchard.Data;
using Orchard.Models;

namespace Orchard.Blogs.Services {
    public class BlogPostService : IBlogPostService {
        private readonly IContentManager _contentManager;
        private readonly IRepository<RoutableRecord> _routableRepository;

        public BlogPostService(IContentManager contentManager, IRepository<RoutableRecord> routableRepository) {
            _contentManager = contentManager;
            _routableRepository = routableRepository;
        }

        public BlogPost Get(Blog blog, string slug) {
            RoutableRecord record =
                _routableRepository.Get(r => r.ContentItem.ContentType.Name == "blogpost" && r.Slug == slug);
            BlogPost blogPost = record != null ? _contentManager.Get<BlogPost>(record.Id) : null;

            return blogPost != null && blogPost.Blog.ContentItem.Id == blog.ContentItem.Id ? blogPost : null;
        }

        public IEnumerable<BlogPost> Get(Blog blog) {
            //TODO: (erikpo) Figure out how to sort by published date
            IEnumerable<RoutableRecord> records =
                _routableRepository.Fetch(rr => rr.ContentItem.ContentType.Name == "blogpost"
                    /*, bpr => bpr.Asc(bpr2 => bpr2.Published.GetValueOrDefault(new DateTime(2099, 1, 1)))*/);

            //TODO: (erikpo) Need to filter by blog in the line above instead of manually here
            return
                records.Select(r => _contentManager.Get(r.Id).As<BlogPost>()).Where(
                    bp => bp.Blog.ContentItem.Id == blog.ContentItem.Id);
        }

        public BlogPost Create(CreateBlogPostParams parameters) {
            return _contentManager.Create<BlogPost>("blogpost", bp =>
            {
                bp.Record.Blog = parameters.Blog.Record;
                bp.Record.Published = parameters.Published;
                bp.As<RoutableAspect>().Record.Title = parameters.Title;
                bp.As<RoutableAspect>().Record.Slug = parameters.Slug;
            });
        }
    }
}