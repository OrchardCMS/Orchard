using System;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Security.Policy;
using System.Web.Mvc;
using System.Web.Routing;
using System.ServiceModel.Syndication;
using PackageIndexReferenceImplementation.Controllers.Artifacts;
using PackageIndexReferenceImplementation.Services;

namespace PackageIndexReferenceImplementation.Controllers {
    [HandleError]
    public class AtomController : Controller {
        private readonly FeedStorage _feedStorage;
        private readonly MediaStorage _mediaStorage;

        public AtomController() {
            _feedStorage = new FeedStorage();
            _mediaStorage = new MediaStorage();
        }

        public ActionResult Index() {
            return new AtomFeedResult(_feedStorage.GetFeed());
        }

        [ActionName("Index"), HttpPost, ContentType("application/atom+xml"), XmlBody]
        public ActionResult PostEntry(string body) {
            return RedirectToAction("Index");
        }

        [ActionName("Index"), HttpPost, ContentType("application/x-package")]
        public ActionResult PostPackage() {

            var hostHeader = HttpContext.Request.Headers["Host"];
            var slugHeader = HttpContext.Request.Headers["Slug"];
            var utcNowDateString = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");

            var package = Package.Open(Request.InputStream, FileMode.Open, FileAccess.Read);
            var packageProperties = package.PackageProperties;

            var feed = _feedStorage.GetFeed();

            var item = feed.Items.FirstOrDefault(i => i.Id.StartsWith("tag:") && i.Id.EndsWith(":" + packageProperties.Identifier));
            if (item == null) {
                item = new SyndicationItem {
                    Id = "tag:" + hostHeader + "," + utcNowDateString + ":" + packageProperties.Identifier
                };
                feed.Items = feed.Items.Concat(new[] { item });
            }


            if (!string.IsNullOrEmpty(packageProperties.Category)) {
                item.Authors.Clear();
                //parse package.PackageProperties.Creator into email-style authors
                item.Authors.Add(new SyndicationPerson { Name = packageProperties.Creator });
            }

            if (!string.IsNullOrEmpty(packageProperties.Category)) {
                item.Categories.Clear();
                item.Categories.Add(new SyndicationCategory(packageProperties.Category));
            }

            if (packageProperties.Modified.HasValue) {
                item.LastUpdatedTime = new DateTimeOffset(packageProperties.Modified.Value);
            }

            if (!string.IsNullOrEmpty(packageProperties.Title)) {
                item.Title = new TextSyndicationContent(packageProperties.Title);
            }

            if (!string.IsNullOrEmpty(packageProperties.Description)) {
                item.Summary = new TextSyndicationContent(packageProperties.Description);
            }

            if (!string.IsNullOrEmpty(packageProperties.Title)) {
                item.Title = new TextSyndicationContent(packageProperties.Title);
            }

            var mediaIdentifier = packageProperties.Identifier + "-" + packageProperties.Version + ".zip";

            var mediaUrl = Url.Action("Resource", "Media", new RouteValueDictionary { { "Id", mediaIdentifier }, { "ContentType", "application/x-package" } });
            item.Links.Clear();
            item.Links.Add(new SyndicationLink(new Uri(HostBaseUri(), new Uri(mediaUrl, UriKind.Relative))));

            Request.InputStream.Seek(0, SeekOrigin.Begin);
            _mediaStorage.StoreMedia(mediaIdentifier+":application/x-package", Request.InputStream);
            _feedStorage.StoreFeed(feed);

            return new AtomItemResult("201 Created", null, item);
        }

        private Uri HostBaseUri() {
            return new Uri("http://" + HttpContext.Request.Headers["Host"]);
        }

    }
}
