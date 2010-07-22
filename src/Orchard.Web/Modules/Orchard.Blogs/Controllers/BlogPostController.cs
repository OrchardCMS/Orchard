using System.Linq;
using System.Web.Mvc;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;
using Orchard.ContentManagement;
using Orchard.Core.Feeds;
using Orchard.Localization;
using Orchard.Mvc.Results;
using Orchard.Security;

namespace Orchard.Blogs.Controllers {
    public class BlogPostController : Controller {
        private readonly IOrchardServices _services;
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;
        private readonly IFeedManager _feedManager;

        public BlogPostController(
            IOrchardServices services, 
            IBlogService blogService, 
            IBlogPostService blogPostService,
            IFeedManager feedManager) {
            _services = services;
            _blogService = blogService;
            _blogPostService = blogPostService;
            _feedManager = feedManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        //TODO: (erikpo) Should think about moving the slug parameters and get calls and null checks up into a model binder or action filter
        public ActionResult Item(string blogSlug, string postSlug) {
            if (!_services.Authorizer.Authorize(StandardPermissions.AccessFrontEnd, T("Couldn't view blog post")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            BlogPart blogPart = _blogService.Get(blogSlug);

            if (blogPart == null)
                return new NotFoundResult();

            //TODO: (erikpo) Look up the current user and their permissions to this blog post and determine if they should be able to view it or not.
            VersionOptions versionOptions = VersionOptions.Published;
            BlogPostPart postPart = _blogPostService.Get(blogPart, postSlug, versionOptions);

            if (postPart == null)
                return new NotFoundResult();

            var model = new BlogPostViewModel {
                BlogPart = blogPart,
                BlogPost = _services.ContentManager.BuildDisplayModel(postPart, "Detail")
            };

            _feedManager.Register(blogPart);

            return View(model);
        }

        public ActionResult ListByArchive(string blogSlug, string archiveData) {
            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            BlogPart blogPart = _blogService.Get(blogSlug);

            if (blogPart == null)
                return new NotFoundResult();

            var archive = new ArchiveData(archiveData);
            var model = new BlogPostArchiveViewModel {
                BlogPart = blogPart,
                ArchiveData = archive,
                BlogPosts = _blogPostService.Get(blogPart, archive).Select(b => _services.ContentManager.BuildDisplayModel(b, "Summary"))
            };

            _feedManager.Register(blogPart);

            return View(model);
        }
    }
}