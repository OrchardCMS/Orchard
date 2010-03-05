using System.Linq;
using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Routing;
using Orchard.Blogs.ViewModels;
using Orchard.Mvc.Results;
using Orchard.Services;
using Orchard.Core.Feeds;

namespace Orchard.Blogs.Services {
    [UsedImplicitly]
    public class BlogHomePageProvider : IHomePageProvider {
        private readonly IBlogService _blogService;
        private readonly IBlogSlugConstraint _blogSlugConstraint;
        private readonly IFeedManager _feedManager;

        public BlogHomePageProvider(IOrchardServices services, IBlogService blogService, IBlogSlugConstraint blogSlugConstraint, IFeedManager feedManager) {
            Services = services;
            _blogService = blogService;
            _blogSlugConstraint = blogSlugConstraint;
            _feedManager = feedManager;
        }

        public IOrchardServices Services { get; private set; }

        public string GetProviderName() {
            return "BlogHomePageProvider";
        }

        public ActionResult GetHomePage(int itemId) {
            var blog = _blogService.Get().Where(x => x.Id == itemId).FirstOrDefault();
            if (blog == null)
                return new NotFoundResult();

            var correctedSlug = _blogSlugConstraint.FindSlug(blog.Slug);
            if (correctedSlug == null)
                return new NotFoundResult();

            blog = _blogService.Get(correctedSlug);
            if (blog == null)
                return new NotFoundResult();

            var model = new BlogViewModel {
                Blog = Services.ContentManager.BuildDisplayModel(blog, "Detail")
            };

            _feedManager.Register(blog);

            return new ViewResult {
                ViewName = "~/Modules/Orchard.Blogs/Views/Blog/Item.ascx",
                ViewData = new ViewDataDictionary<BlogViewModel>(model)
            };
        }
    }
}
