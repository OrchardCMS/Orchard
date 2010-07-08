using System.ServiceModel.Syndication;
using System.Web.Mvc;
using System.Xml;

namespace PackageIndexReferenceImplementation.Controllers.Artifacts {
    public class AtomItemResult : ActionResult {
        public string Status { get; set; }
        public string Location { get; set; }
        public SyndicationItem Item { get; set; }

        public AtomItemResult(string status, string location, SyndicationItem item) {
            Status = status;
            Location = location;
            Item = item;
        }

        public override void ExecuteResult(ControllerContext context) {
            context.HttpContext.Response.Status = Status;
            context.HttpContext.Response.RedirectLocation = Location;
            context.HttpContext.Response.ContentType = "application/atom+xml;type=entry";
            using (var writer = XmlWriter.Create(context.HttpContext.Response.OutputStream)) {
                var formatter = new Atom10ItemFormatter(Item);
                formatter.WriteTo(writer);
            }
        }
    }
}