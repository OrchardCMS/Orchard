using System;
using System.Web;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Media;
using Orchard.Media.Services;
using Orchard.Media.ViewModels;
using Orchard.Themes;

namespace Orchard.MediaPicker.Controllers {
    [Themed(false)]
    public class AdminController : Controller {
        private readonly IMediaService _mediaService;

        public IOrchardServices Services { get; set; }

        public AdminController(IOrchardServices services, IMediaService mediaService) {
            Services = services;
            _mediaService = mediaService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index(string name, string mediaPath) {
            var mediaFolders = _mediaService.GetMediaFolders(mediaPath);
            var mediaFiles = string.IsNullOrEmpty(mediaPath) ? null : _mediaService.GetMediaFiles(mediaPath);
            var model = new MediaFolderEditViewModel { FolderName = name, MediaFiles = mediaFiles, MediaFolders = mediaFolders, MediaPath = mediaPath };
            ViewData["Service"] = _mediaService;
            return View(model);
        }

        [HttpPost]
        public JsonResult CreateFolder(string path, string folderName) {
            if (!Services.Authorizer.Authorize(Permissions.ManageMedia)) {
                return Json(new { Success = false, Message = T("Couldn't create media folder").ToString() });
            }

            try {
                _mediaService.CreateFolder(HttpUtility.UrlDecode(path), folderName);
                return Json(new { Success = true, Message = "" });
            }
            catch (Exception exception) {
                return Json(new { Success = false, Message = T("Creating Folder failed: {0}", exception.Message).ToString()} );
            }
        }
    }
}
