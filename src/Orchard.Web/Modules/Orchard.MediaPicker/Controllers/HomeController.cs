using System.Web.Mvc;
using Orchard.Localization;
using Orchard;
using System.Collections.Generic;
using Orchard.Media.Models;
using Orchard.Media.ViewModels;
using Orchard.Media.Services;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.Themes;

namespace Orchard.MediaPicker.Controllers {
    [Themed(false)]
    public class HomeController : Controller {
        private readonly IMediaService _mediaService;
        private readonly IAuthorizer _authorizer;

        public IOrchardServices Services { get; set; }

        public HomeController(IOrchardServices services, IMediaService mediaService, IAuthorizer authorizer) {
            _authorizer = authorizer;

            Services = services;
            _mediaService = mediaService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index(string name, string mediaPath) {
            // this controller should only be used from the admin panel.
            // it is not itself an admincontroller, however, because it needs to be 'unthemed',
            // which admincontrollers currently cannot be.
            if (!_authorizer.Authorize(StandardPermissions.AccessAdminPanel, T("Can't access the admin"))) {
                return new HttpUnauthorizedResult();
            }

            IEnumerable<MediaFile> mediaFiles = _mediaService.GetMediaFiles(mediaPath);
            IEnumerable<MediaFolder> mediaFolders = _mediaService.GetMediaFolders(mediaPath);
            var model = new MediaFolderEditViewModel { FolderName = name, MediaFiles = mediaFiles, MediaFolders = mediaFolders, MediaPath = mediaPath };
            ViewData["Service"] = _mediaService;
            return View(model);
        }
    }
}
