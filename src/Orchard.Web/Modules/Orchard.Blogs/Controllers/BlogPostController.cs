using System.Linq;
using System.Web.Mvc;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.Core.Feeds;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Themes;

namespace Orchard.Blogs.Controllers {
    [Themed]
    public class BlogPostController : Controller {
        private readonly IOrchardServices _services;
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;
        private readonly IFeedManager _feedManager;

        public BlogPostController(
            IOrchardServices services, 
            IBlogService blogService, 
            IBlogPostService blogPostService,
            IFeedManager feedManager,
            IShapeFactory shapeFactory) {
            _services = services;
            _blogService = blogService;
            _blogPostService = blogPostService;
            _feedManager = feedManager;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }

        //TODO: (erikpo) Should think about moving the slug parameters and get calls and null checks up into a model binder or action filter
        public ActionResult Item(string blogPath, string postSlug) {
            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            var blogPart = _blogService.Get(blogPath);
            if (blogPart == null)
                return HttpNotFound();

            //TODO: (erikpo) Look up the current user and their permissions to this blog post and determine if they should be able to view it or not.
            var postPart = _blogPostService.Get(blogPart, postSlug, VersionOptions.Published);
            if (postPart == null)
                return HttpNotFound();

            dynamic model = _services.ContentManager.BuildDisplay(postPart);
            return new ShapeResult(this, model);
        }

        public ActionResult ListByArchive(string blogPath, string archiveData) {
            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            BlogPart blogPart = _blogService.Get(blogPath);

            if (blogPart == null)
                return HttpNotFound();

            var archive = new ArchiveData(archiveData);

            var list = Shape.List();
            list.AddRange(_blogPostService.Get(blogPart, archive).Select(b => _services.ContentManager.BuildDisplay(b, "Summary")));

            _feedManager.Register(blogPart);

            dynamic viewModel = Shape.ViewModel()
                .ContentItems(list)
                .Blog(blogPart)
                .ArchiveData(archive);

            //todo: (heskew) add back
            //.ArchiveData(archive) <-- ??

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)viewModel);
        }
    }
}