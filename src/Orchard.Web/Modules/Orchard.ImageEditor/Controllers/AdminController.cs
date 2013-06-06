using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.FileSystems.Media;
using Orchard.ImageEditor.Models;
using Orchard.MediaLibrary.Models;
using Orchard.Mvc;
using Orchard.Themes;
using Orchard.UI.Admin;

namespace Orchard.ImageEditor.Controllers {
    [Admin]
    public class AdminController : Controller {
        private readonly IStorageProvider _storageProvider;

        public AdminController(IOrchardServices orchardServices, IStorageProvider storageProvider) {
            _storageProvider = storageProvider;
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
        public ActionResult Touch(string src) {
            var localPath = _storageProvider.GetLocalPath(src);

            if (_storageProvider.GetFile(localPath) == null) {
                return HttpNotFound();
            }

            var media = Services.ContentManager.Query<MediaPart, MediaPartRecord>().Where(x => x.Resource == src).Slice(0, 1).FirstOrDefault();

            return Json(media != null);
        }

        [Themed(false)]
        public ActionResult Edit(string src) {
            var localPath = _storageProvider.GetLocalPath(src);

            if (_storageProvider.GetFile(localPath) == null) {
                return HttpNotFound();
            }

            var media = Services.ContentManager.Query<MediaPart, MediaPartRecord>().Where(x => x.Resource == src).Slice(0, 1).FirstOrDefault();

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
            var media = Services.ContentManager.Get(id).As<MediaPart>();

            if (media == null) {
                return HttpNotFound();
            }

            const string signature = "data:image/png;base64,";

            if (!content.StartsWith(signature, StringComparison.OrdinalIgnoreCase)) {
                return HttpNotFound();
            }

            var image = media.As<ImagePart>();

            content = content.Substring(signature.Length);

            var buffer = Convert.FromBase64String(content);
            var localPath = _storageProvider.GetLocalPath(media.Resource);
            _storageProvider.DeleteFile(localPath);
            using (var stream = new MemoryStream(buffer)) {
                _storageProvider.SaveStream(localPath, stream);
            }

            image.Width = width;
            image.Height = height;
            media.MimeType = "image/png";

            return Json(true);
        }
    }
}