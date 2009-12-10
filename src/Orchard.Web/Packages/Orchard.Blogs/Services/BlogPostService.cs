using System.Collections.Generic;
using System.Linq;
using Orchard.Blogs.Models;
using Orchard.Core.Common.Records;
using Orchard.Data;
using Orchard.Models;

namespace Orchard.Blogs.Services {
    public class BlogPostService : IBlogPostService {
        private readonly IContentManager _contentManager;
        private readonly IRepository<BlogPostRecord> _blogPostRepository;

        public BlogPostService(IContentManager contentManager, IRepository<BlogPostRecord> blogPostRepository) {
            _contentManager = contentManager;
            _blogPostRepository = blogPostRepository;
        }

        public BlogPost Get(Blog blog, string slug) {
            return _contentManager.Query<BlogPost, BlogPostRecord >()
                .Join<RoutableRecord>().Where(x => x.Slug == slug)
                .Join<CommonRecord>().Where(x => x.Container == blog.Record.ContentItemRecord)
                .List().FirstOrDefault();
        }

        public IEnumerable<BlogPost> Get(Blog blog) {
            return _contentManager.Query<BlogPost, BlogPostRecord>()
                .Join<CommonRecord>().Where(x => x.Container == blog.Record.ContentItemRecord)
                .OrderByDescending(x=>x.CreatedUtc)
                .List();
        }

        public void Delete(BlogPost blogPost) {
            _blogPostRepository.Delete(blogPost.Record);
        }
    }
}