using System.Collections.Generic;
using System.Linq;
using Orchard.Blogs.Models;
using Orchard.Data;
using Orchard.Models;

namespace Orchard.Blogs.Services {
    public class BlogService : IBlogService {
        private readonly IContentManager _contentManager;
        private readonly IRepository<BlogRecord> _repository;

        public BlogService(IContentManager contentManager, IRepository<BlogRecord> repository) {
            _contentManager = contentManager;
            _repository = repository;
        }

        public Blog Get(string slug) {
            BlogRecord record = _repository.Get(br => br.Slug == slug && br.Enabled);
            return _contentManager.Get<Blog>(record.Id);
        }

        public IEnumerable<Blog> Get() {
            IEnumerable<BlogRecord> blogs = _repository.Fetch(br => br.Enabled, br => br.Asc(br2 => br2.Name));

            return blogs.Select(br => _contentManager.Get<Blog>(br.Id));
        }

        public Blog CreateBlog(CreateBlogParams parameters) {
            return _contentManager.Create<Blog>("blog", init => {
                init.Record.Name = parameters.Name;
                init.Record.Slug = parameters.Slug;
                init.Record.Enabled = parameters.Enabled;
            });
        }
    }
}