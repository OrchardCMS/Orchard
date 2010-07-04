using System;
using System.IO;
using System.IO.Packaging;
using System.Reflection;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using System.ServiceModel.Syndication;

namespace PackageIndexReferenceImplementation.Controllers {
    public class SyndicationResult : ActionResult {
        public SyndicationFeedFormatter Formatter { get; set; }

        public SyndicationResult(SyndicationFeedFormatter formatter) {
            Formatter = formatter;
        }

        public override void ExecuteResult(ControllerContext context) {
            context.HttpContext.Response.ContentType = "application/atom+xml";
            using (var writer = XmlWriter.Create(context.HttpContext.Response.OutputStream)) {
                Formatter.WriteTo(writer);
            }
        }
    }

    [HandleError]
    public class AtomController : Controller {
        public ActionResult Index() {
            var feed = new SyndicationFeed {
                Items = new[] {
                    new SyndicationItem {
                        Id = "hello",
                        Title = new TextSyndicationContent("Orchard.Media", TextSyndicationContentKind.Plaintext),
                        LastUpdatedTime = DateTimeOffset.UtcNow
                    }
                }
            };

            return new SyndicationResult(new Atom10FeedFormatter(feed));
        }

        [ActionName("Index"), HttpPost, ContentType("application/atom+xml"), XmlBody]
        public ActionResult PostEntry(string body) {
            return RedirectToAction("Index");
        }

        [ActionName("Index"), HttpPost, ContentType("application/x-package")]
        public ActionResult PostPackage() {
            var package = Package.Open(Request.InputStream, FileMode.Open, FileAccess.Read);

            return RedirectToAction("Index");
        }

        static XElement Atom(string localName, params XNode[] content) {
            return new XElement(XName.Get(localName, "http://www.w3.org/2005/Atom"), content);
        }
        static XElement Atom(string localName, string value) {
            return new XElement(XName.Get(localName, "http://www.w3.org/2005/Atom"), new XText(value));
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ContentTypeAttribute : ActionMethodSelectorAttribute {
        public ContentTypeAttribute(string contentType) {
            ContentType = contentType;
        }

        public string ContentType { get; set; }

        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo) {
            return controllerContext.HttpContext.Request.ContentType.StartsWith(ContentType);
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class XmlBodyAttribute : ActionFilterAttribute {
        public override void OnActionExecuting(ActionExecutingContext filterContext) {
            var body = XElement.Load(filterContext.HttpContext.Request.InputStream);
            filterContext.ActionParameters["body"] = body.ToString();
        }
    }
}
