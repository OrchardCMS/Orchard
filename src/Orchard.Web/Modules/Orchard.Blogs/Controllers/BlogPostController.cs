using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.Blogs.Routing;
using Orchard.Blogs.Services;
using Orchard.Core.Feeds;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Themes;

namespace Orchard.Blogs.Controllers {
    [Themed]
    public class BlogPostController : Controller {
        private readonly IOrchardServices _services;
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;
        private readonly IFeedManager _feedManager;
        private readonly IArchiveConstraint _archiveConstraint;

        public BlogPostController(
            IOrchardServices services, 
            IBlogService blogService, 
            IBlogPostService blogPostService,
            IFeedManager feedManager,
            IShapeFactory shapeFactory,
            IArchiveConstraint archiveConstraint) {
            _services = services;
            _blogService = blogService;
            _blogPostService = blogPostService;
            _feedManager = feedManager;
            _archiveConstraint = archiveConstraint;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ActionResult ListByArchive(string path) {

            var blogPath = _archiveConstraint.FindPath(path);
            var archive = _archiveConstraint.FindArchiveData(path);

            if (blogPath == null)
                return HttpNotFound();

            if (archive == null)
                return HttpNotFound();

            BlogPart blogPart = _blogService.Get(blogPath);
            if (blogPart == null)
                return HttpNotFound();


            if (archive.ToDateTime() == DateTime.MinValue) {
                // render the archive data
                return new ShapeResult(this, Shape.Parts_Blogs_BlogArchives(Blog: blogPart, Archives: _blogPostService.GetArchives(blogPart)));
            }

            var list = Shape.List();
            list.AddRange(_blogPostService.Get(blogPart, archive).Select(b => _services.ContentManager.BuildDisplay(b, "Summary")));

            _feedManager.Register(blogPart, _services.ContentManager.GetItemMetadata(blogPart).DisplayText);

            var viewModel = Shape.ViewModel()
                .ContentItems(list)
                .Blog(blogPart)
                .ArchiveData(archive);

            return View(viewModel);
        }
    }
}