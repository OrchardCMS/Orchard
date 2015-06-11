using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard.Logging;
using Orchard.Security;

namespace Orchard.Core.XmlRpc.Controllers {
    public class LiveWriterController : Controller {
        private readonly IEnumerable<IXmlRpcHandler> _xmlRpcHandlers;
        private const string ManifestUri = "http://schemas.microsoft.com/wlw/manifest/weblog";

        public LiveWriterController(IEnumerable<IXmlRpcHandler> xmlRpcHandlers) {
            _xmlRpcHandlers = xmlRpcHandlers;
            Logger = NullLogger.Instance;
        }

        protected ILogger Logger { get; set; }
        
        [NoCache]
        [AlwaysAccessible]
        public ActionResult Manifest() {
            Logger.Debug("Manifest requested");

            var options = new XElement(
                XName.Get("options", ManifestUri),
                new XElement(XName.Get("supportsAutoUpdate", ManifestUri), "Yes"),
                new XElement(XName.Get("clientType", ManifestUri), "Metaweblog"),
                new XElement(XName.Get("supportsKeywords", ManifestUri), "No"),
                new XElement(XName.Get("supportsCategories", ManifestUri), "No"),
                new XElement(XName.Get("supportsFileUpload", ManifestUri), "No"),
                new XElement(XName.Get("supportsCustomDate", ManifestUri), "No"));

            foreach (var handler in _xmlRpcHandlers)
                handler.SetCapabilities(options);

            var doc = new XDocument(new XElement(
                                        XName.Get("manifest", ManifestUri),
                                        options));

            return Content(doc.ToString(), "text/xml");
        }

        public class NoCache : ActionFilterAttribute {
            public override void OnResultExecuting(ResultExecutingContext filterContext) {
                filterContext.HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
                filterContext.HttpContext.Response.Cache.SetValidUntilExpires(false);
                filterContext.HttpContext.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
                filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                filterContext.HttpContext.Response.Cache.SetNoStore();
                base.OnResultExecuting(filterContext);
            }
        }
    }
}