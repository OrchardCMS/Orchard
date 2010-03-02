using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;
using Orchard.Core.Feeds;
using Orchard.Logging;
using Orchard.Mvc.Results;

namespace Orchard.Blogs.Controllers {
    public class BlogController : Controller {
        private readonly IOrchardServices _services;
        private readonly IBlogService _blogService;
        private readonly IFeedManager _feedManager;

        public BlogController(
            IOrchardServices services,
            IBlogService blogService,
            IFeedManager feedManager) {
            _services = services;
            _blogService = blogService;
            _feedManager = feedManager;
            Logger = NullLogger.Instance;
        }

        protected ILogger Logger { get; set; }

        public ActionResult List() {
            var model = new BlogsViewModel {
                Blogs = _blogService.Get().Select(b => _services.ContentManager.BuildDisplayModel(b, "Summary"))
            };

            return View(model);
        }

        //TODO: (erikpo) Should move the slug parameter and get call and null check up into a model binder
        public ActionResult Item(string blogSlug) {
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            var model = new BlogViewModel {
                Blog = _services.ContentManager.BuildDisplayModel(blog, "Detail")
            };

            _feedManager.Register(blog);

            return View(model);
        }

        public ActionResult LiveWriterManifest() {
            Logger.Debug("Live Writer Manifest requested");

            const string manifestUri = "http://schemas.microsoft.com/wlw/manifest/weblog";

            var options = new XElement(
                XName.Get("options", manifestUri),
                new XElement(XName.Get("clientType", manifestUri), "Metaweblog"),
                new XElement(XName.Get("supportsSlug", manifestUri), "Yes"));

            var doc = new XDocument(new XElement(
                                        XName.Get("manifest", manifestUri),
                                        options));

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return Content(doc.ToString(), "text/xml");
        }
    }
}