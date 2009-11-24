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
        private readonly IRepository<BlogRecord> _repository;
        private readonly IRepository<RoutableRecord> _routableRepository;

        public BlogService(IContentManager contentManager, IRepository<BlogRecord> repository, IRepository<RoutableRecord> routableRepository) {
            _contentManager = contentManager;
            _repository = repository;
            _routableRepository = routableRepository;
        }

        public Blog Get(string slug) {
            RoutableRecord record = _routableRepository.Get(r => r.ContentItem.ContentType.Name == "blog" && r.Slug == slug);

            return record != null ?_contentManager.Get<Blog>(record.Id) : null;
        }

        public IEnumerable<Blog> Get() {
            IEnumerable<RoutableRecord> records = _routableRepository.Fetch(rr => rr.ContentItem.ContentType.Name == "blog", rr => rr.Asc(rr2 => rr2.Title));

            return records.Select(rr => _contentManager.Get<Blog>(rr.Id));
        }

        public Blog CreateBlog(CreateBlogParams parameters) {
            return _contentManager.Create<Blog>("blog", init => {
                init.Record.Description = parameters.Description;
                init.As<RoutableAspect>().Record.Title = parameters.Name;
                init.As<RoutableAspect>().Record.Slug = parameters.Slug;
            });
        }
    }
}