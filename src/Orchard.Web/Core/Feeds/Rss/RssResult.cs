using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;

namespace Orchard.Core.Feeds.Rss {
    public class RssResult : ActionResult {
        public XDocument Document { get; private set; }

        public RssResult(XDocument document) {
            Document = document;
        }

        public override void ExecuteResult(ControllerContext context) {
                
            // not returning application/rss+xml because of
            // https://bugzilla.mozilla.org/show_bug.cgi?id=256379
            context.HttpContext.Response.ContentType = "text/xml";

            using (var writer = XmlWriter.Create(context.HttpContext.Response.Output))
                Document.WriteTo(writer);
        }
    }
}