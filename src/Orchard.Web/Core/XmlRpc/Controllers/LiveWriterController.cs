using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard.Logging;

namespace Orchard.Core.XmlRpc.Controllers {
    public class LiveWriterController : Controller {
        private const string ManifestUri = "http://schemas.microsoft.com/wlw/manifest/weblog";

        public LiveWriterController() {
            Logger = NullLogger.Instance;
        }

        protected ILogger Logger { get; set; }

        
        public ActionResult Manifest() {
            Logger.Debug("Manifest requested");

            var options = new XElement(
                XName.Get("options", ManifestUri),
                new XElement(XName.Get("clientType", ManifestUri), "Metaweblog"),
                new XElement(XName.Get("supportsSlug", ManifestUri), "Yes"));

            var doc = new XDocument(new XElement(
                                        XName.Get("manifest", ManifestUri),
                                        options));

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return Content(doc.ToString(), "text/xml");
        }
    }
}