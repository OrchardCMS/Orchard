using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using PackageIndexReferenceImplementation.Services;

namespace PackageIndexReferenceImplementation.Controllers {
    public class MediaController : Controller {  
        private readonly MediaStorage _mediaStorage;

        public MediaController() {
            _mediaStorage = new MediaStorage();
        }

        public ActionResult Resource(string id, string contentType) {
            return new StreamResult(contentType, _mediaStorage.GetMedia(id + ":" + contentType));
        }

        public ActionResult PreviewTheme(string id, string contentType) {
            var stream = _mediaStorage.GetMedia(id + ":" + contentType);
            var package = Package.Open(stream, FileMode.Open, FileAccess.Read);
            if (package.PackageProperties.ContentType != "Orchard Theme") {
                return new HttpNotFoundResult();
            }

            var themeName = package.PackageProperties.Identifier;
            var previewUri = new Uri("/" + themeName + "/Theme.png", UriKind.Relative);
            Stream previewStream;
            DateTime lastModified;
            if (package.PartExists(previewUri)) {
                lastModified = _mediaStorage.GetLastModifiedDate(id);
                previewStream = package.GetPart(new Uri("/" + themeName + "/Theme.png", UriKind.Relative)).GetStream();
            }
            else {
                var defaultPreviewPath = HostingEnvironment.MapPath("~/Content/DefaultThemePreview.png");
                if (defaultPreviewPath == null || !System.IO.File.Exists(defaultPreviewPath)) {
                    return new HttpNotFoundResult();
                }
                lastModified = System.IO.File.GetLastWriteTimeUtc(defaultPreviewPath);
                previewStream = System.IO.File.Open(defaultPreviewPath, FileMode.Open, FileAccess.Read);
            }
            return new StreamResult("image/png", previewStream, lastModified);
        }
    }
}
