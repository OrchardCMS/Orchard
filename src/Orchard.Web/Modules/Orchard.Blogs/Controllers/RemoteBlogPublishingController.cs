using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using Orchard.Blogs.Models;
using Orchard.Blogs.Routing;
using Orchard.Blogs.Services;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Mvc.Extensions;

namespace Orchard.Blogs.Controllers {
    [OrchardFeature("Orchard.Blogs.RemotePublishing")]
    public class RemoteBlogPublishingController : Controller {
        private readonly IBlogService _blogService;
        private readonly IRsdConstraint _rsdConstraint;
        private readonly RouteCollection _routeCollection;

        public RemoteBlogPublishingController(
            IOrchardServices services, 
            IBlogService blogService, 
            IRsdConstraint rsdConstraint,
            RouteCollection routeCollection) {
            _blogService = blogService;
            _rsdConstraint = rsdConstraint;
            _routeCollection = routeCollection;
            Logger = NullLogger.Instance;
        }

        protected ILogger Logger { get; set; }

        public ActionResult Rsd(string path) {
            Logger.Debug("RSD requested");

            var blogPath = _rsdConstraint.FindPath(path);
            
            if (blogPath == null)
                return HttpNotFound();

            BlogPart blogPart = _blogService.Get(blogPath);

            if (blogPart == null)
                return HttpNotFound();

            const string manifestUri = "http://archipelago.phrasewise.com/rsd";

            var urlHelper = new UrlHelper(ControllerContext.RequestContext, _routeCollection);
            var url = urlHelper.AbsoluteAction("", "", new { Area = "XmlRpc" });

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