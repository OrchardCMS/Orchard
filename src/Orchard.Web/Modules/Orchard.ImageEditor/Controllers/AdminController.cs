using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ImageEditor.Models;
using Orchard.MediaLibrary;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Services;
using Orchard.Mvc;
using Orchard.Themes;
using Orchard.UI.Admin;

namespace Orchard.ImageEditor.Controllers {
    [Admin]
    public class AdminController : Controller {
        private readonly IMediaLibraryService _mediaLibraryService;

        public AdminController(IOrchardServices orchardServices, IMediaLibraryService mediaLibraryService) {
            _mediaLibraryService = mediaLibraryService;
            Services = orchardServices;
        }

        public IOrchardServices Services { get; set; }

        public ActionResult Index(int id) {
            var media = Services.ContentManager.Get(id).As<ImagePart>();

            if (media == null) {
                return HttpNotFound();
            }

            var contentItem = Services.ContentManager.New("ImageEditor").As<ImageEditorPart>();

            contentItem.ImagePart = media.As<ImagePart>();
            contentItem.MediaPart = media.As<MediaPart>();

            var shape = Services.ContentManager.BuildDisplay(contentItem);
            shape.MediaContentItem(media.ContentItem);

            return new ShapeResult(this, shape);
        }

        [Themed(false)]
        public ActionResult Edit(string folderPath, string filename) {
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia))
                return new HttpUnauthorizedResult();

            // Check permission.
            var rootMediaFolder = _mediaLibraryService.GetRootMediaFolder();
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent) && !_mediaLibraryService.CanManageMediaFolder(folderPath)) {
                return new HttpUnauthorizedResult();
            }

            var media = Services.ContentManager.Query<MediaPart, MediaPartRecord>().Where(x => x.FolderPath == folderPath && x.FileName == filename).Slice(0, 1).FirstOrDefault();

            if (media == null) {
                return HttpNotFound();
            }

            var contentItem = Services.ContentManager.New("ImageEditor").As<ImageEditorPart>();

            contentItem.ImagePart = media.As<ImagePart>();
            contentItem.MediaPart = media.As<MediaPart>();

            var shape = Services.ContentManager.BuildDisplay(contentItem);
            shape.MediaContentItem(media.ContentItem);

            return View(shape);
        }

        [HttpPost]
        public ActionResult Upload(int id, string content, int width, int height) {
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia))
                return new HttpUnauthorizedResult();

            var media = Services.ContentManager.Get(id).As<MediaPart>();

            if (media == null) {
                return HttpNotFound();
            }

            // Check permission.
            var rootMediaFolder = _mediaLibraryService.GetRootMediaFolder();
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent) && !_mediaLibraryService.CanManageMediaFolder(media.FolderPath)) {
                return new HttpUnauthorizedResult();
            }

            const string signature = "data:image/jpeg;base64,";

            if (!content.StartsWith(signature, StringComparison.OrdinalIgnoreCase)) {
                return HttpNotFound();
            }

            var image = media.As<ImagePart>();

            content = content.Substring(signature.Length);

            var buffer = Convert.FromBase64String(content);

            _mediaLibraryService.DeleteFile(media.FolderPath, media.FileName);

            using (var stream = new MemoryStream(buffer)) {
                _mediaLibraryService.UploadMediaFile(media.FolderPath, media.FileName, stream);
            }

            image.Width = width;
            image.Height = height;
            media.MimeType = "image/png";

            return Json(true);
        }

        public ActionResult Proxy(string url) {
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia))
                return HttpNotFound();

            var sslFailureCallback = new RemoteCertificateValidationCallback((o, cert, chain, errors) => true);

            using (var wc = new WebClient()) {
                try {
                    ServicePointManager.ServerCertificateValidationCallback += sslFailureCallback;

                    var data = wc.DownloadData(url);
                    return new FileContentResult(data, "image");
                }
                catch {
                    return HttpNotFound();
                }
                finally {
                    ServicePointManager.ServerCertificateValidationCallback -= sslFailureCallback;
                }
            }
        }
    }
}