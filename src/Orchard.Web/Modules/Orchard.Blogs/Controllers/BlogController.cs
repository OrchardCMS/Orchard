using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
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
        private readonly RouteCollection _routeCollection;

        public BlogController(
            IOrchardServices services,
            IBlogService blogService,
            IFeedManager feedManager,
            RouteCollection routeCollection) {
            _services = services;
            _blogService = blogService;
            _feedManager = feedManager;
            _routeCollection = routeCollection;
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

        public ActionResult LiveWriterManifest(string blogSlug) {
            Logger.Debug("Live Writer Manifest requested");

            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            const string manifestUri = "http://schemas.microsoft.com/wlw/manifest/weblog";

            var options = new XElement(
                XName.Get("options", manifestUri),
                new XElement(XName.Get("clientType", manifestUri), "Metaweblog"),
                new XElement(XName.Get("supportsSlug", manifestUri), "Yes"),
                new XElement(XName.Get("supportsKeywords", manifestUri), "Yes"));


            var doc = new XDocument(new XElement(
                                        XName.Get("manifest", manifestUri),
                                        options));

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return Content(doc.ToString(), "text/xml");
        }

        public ActionResult Rsd(string blogSlug) {
            Logger.Debug("RSD requested");

            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            const string manifestUri = "http://archipelago.phrasewise.com/rsd";

            var urlHelper = new UrlHelper(ControllerContext.RequestContext, _routeCollection);
            var url = urlHelper.Action("", "", new { Area = "XmlRpc" });

            var options = new XElement(
                XName.Get("service", manifestUri),
                new XElement(XName.Get("engineName", manifestUri), "Orchar CMS"),
                new XElement(XName.Get("engineLink", manifestUri), "http://orchardproject.net"),
                new XElement(XName.Get("homePageLink", manifestUri), "http://orchardproject.net"),
                new XElement(XName.Get("apis", manifestUri),
                    new XElement(XName.Get("api", manifestUri),
                        new XAttribute("name", "MetaWeblog"),
                        new XAttribute("preferred", true),
                        new XAttribute("apiLink", url),
                        new XAttribute("blogID", blog.Id))));

            var doc = new XDocument(new XElement(
                                        XName.Get("rsd", manifestUri),
                                        new XAttribute("version", "1.0"),
                                        options));

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return Content(doc.ToString(), "text/xml");
        }
    }
}