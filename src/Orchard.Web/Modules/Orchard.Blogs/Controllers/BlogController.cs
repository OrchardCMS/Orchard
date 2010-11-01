using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.Blogs.Routing;
using Orchard.Blogs.Services;
using Orchard.Core.Feeds;
using Orchard.DisplayManagement;
using Orchard.Logging;
using Orchard.Themes;
using Orchard.UI.Navigation;

namespace Orchard.Blogs.Controllers {
    [Themed]
    public class BlogController : Controller {
        private readonly IOrchardServices _services;
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;
        private readonly IBlogSlugConstraint _blogSlugConstraint;
        private readonly IFeedManager _feedManager;
        private readonly RouteCollection _routeCollection;

        public BlogController(
            IOrchardServices services, 
            IBlogService blogService,
            IBlogPostService blogPostService,
            IBlogSlugConstraint blogSlugConstraint,
            IFeedManager feedManager,
            RouteCollection routeCollection, 
            IShapeFactory shapeFactory) {
            _services = services;
            _blogService = blogService;
            _blogPostService = blogPostService;
            _blogSlugConstraint = blogSlugConstraint;
            _feedManager = feedManager;
            _routeCollection = routeCollection;
            Logger = NullLogger.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        protected ILogger Logger { get; set; }

        public ActionResult List() {
            var blogs = _blogService.Get().Select(b => _services.ContentManager.BuildDisplay(b, "Summary"));

            var list = Shape.List();
            list.AddRange(blogs);

            var viewModel = Shape.ViewModel()
                .ContentItems(list);

            return View(viewModel);
        }

        public ActionResult Item(string blogSlug, Pager pager) {
            var correctedSlug = _blogSlugConstraint.FindSlug(blogSlug);
            if (correctedSlug == null)
                return HttpNotFound();

            var blogPart = _blogService.Get(correctedSlug);
            if (blogPart == null)
                return HttpNotFound();

            _feedManager.Register(blogPart);
            var blogPosts = _blogPostService.Get(blogPart, pager.GetStartIndex(), pager.PageSize)
                .Select(b => _services.ContentManager.BuildDisplay(b, "Summary"));
            var blog = _services.ContentManager.BuildDisplay(blogPart);

            var list = Shape.List();
            list.AddRange(blogPosts);
            blog.Content.Add(Shape.Parts_Blogs_BlogPost_List(ContentItems: list), "5");

            var hasNextPage = _blogPostService.Get(blogPart, pager.GetStartIndex(pager.Page + 1), 1).Any();
            blog.Content.Add(Shape.Pager(pager).HasNextPage(hasNextPage), "Content:after");

            return View(blog);
        }

        public ActionResult LiveWriterManifest(string blogSlug) {
            Logger.Debug("Live Writer Manifest requested");

            BlogPart blogPart = _blogService.Get(blogSlug);

            if (blogPart == null)
                return HttpNotFound();

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

            BlogPart blogPart = _blogService.Get(blogSlug);

            if (blogPart == null)
                return HttpNotFound();

            const string manifestUri = "http://archipelago.phrasewise.com/rsd";

            var urlHelper = new UrlHelper(ControllerContext.RequestContext, _routeCollection);
            var url = urlHelper.Action("", "", new { Area = "XmlRpc" });

            var options = new XElement(
                XName.Get("service", manifestUri),
                new XElement(XName.Get("engineName", manifestUri), "Orchard CMS"),
                new XElement(XName.Get("engineLink", manifestUri), "http://orchardproject.net"),
                new XElement(XName.Get("homePageLink", manifestUri), "http://orchardproject.net"),
                new XElement(XName.Get("apis", manifestUri),
                    new XElement(XName.Get("api", manifestUri),
                        new XAttribute("name", "MetaWeblog"),
                        new XAttribute("preferred", true),
                        new XAttribute("apiLink", url),
                        new XAttribute("blogID", blogPart.Id))));

            var doc = new XDocument(new XElement(
                                        XName.Get("rsd", manifestUri),
                                        new XAttribute("version", "1.0"),
                                        options));

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return Content(doc.ToString(), "text/xml");
        }
    }
}