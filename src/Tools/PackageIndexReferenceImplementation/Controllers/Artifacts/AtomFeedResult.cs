using System.ServiceModel.Syndication;
using System.Web.Mvc;
using System.Xml;

namespace PackageIndexReferenceImplementation.Controllers.Artifacts {
    public class AtomFeedResult : ActionResult {
        public SyndicationFeed Feed { get; set; }

        public AtomFeedResult(SyndicationFeed feed) {
            Feed = feed;
        }

        public override void ExecuteResult(ControllerContext context) {
            context.HttpContext.Response.ContentType = "application/atom+xml";
            using (var writer = XmlWriter.Create(context.HttpContext.Response.OutputStream)) {
                var formatter = new Atom10FeedFormatter(Feed);
                formatter.WriteTo(writer);
            }
        }
    }
}