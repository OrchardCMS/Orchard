using System.Collections.Generic;
using System.Linq;
using Orchard.Blogs.Models;
using Orchard.Data;
using Orchard.Models;

namespace Orchard.Blogs.Services {
    public class BlogPostService : IBlogPostService {
        private readonly IContentManager _contentManager;
        private readonly IRepository<BlogPostRecord> _repository;

        public BlogPostService(IContentManager contentManager, IRepository<BlogPostRecord> repository) {
            _contentManager = contentManager;
            _repository = repository;
        }

        public BlogPost Get(Blog blog, string slug) {
            BlogPostRecord record = _repository.Get(bpr => bpr.Blog.Id == blog.Record.Id/* && bpr.Blog.Enabled*/ && bpr.Slug == slug);

            return _contentManager.Get<BlogPost>(record.Id);
        }

        public IEnumerable<BlogPost> Get(Blog blog) {
            //TODO: (erikpo) Sort by published desc
            IEnumerable<BlogPostRecord> items =_repository.Fetch(bpr => bpr.Blog.Id == blog.Record.Id/* && bpr.Blog.Enabled, bpr => bpr.Asc(bpr2 => bpr2.Slug)*/);

            return items.Select(br => _contentManager.Get(br.Id).As<BlogPost>());
        }
    }
}