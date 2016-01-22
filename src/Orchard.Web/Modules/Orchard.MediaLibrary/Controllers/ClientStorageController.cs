using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.MediaLibrary.Services;
using Orchard.MediaLibrary.ViewModels;
using Orchard.Themes;
using Orchard.UI.Admin;

namespace Orchard.MediaLibrary.Controllers {
    [Admin, Themed(false)]
    public class ClientStorageController : Controller {
        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly IContentManager _contentManager;

        public ClientStorageController(
            IMediaLibraryService mediaManagerService, 
            IContentManager contentManager,
            IOrchardServices orchardServices) {
            _mediaLibraryService = mediaManagerService;
            _contentManager = contentManager;
            Services = orchardServices;
        }

        public IOrchardServices Services { get; set; }

        public ActionResult Index(string folderPath, string type) {

            var viewModel = new ImportMediaViewModel {
                FolderPath = folderPath,
                Type = type
            };

            return View(viewModel);
        }
        
        [HttpPost]
        public ActionResult Upload(string folderPath, string type) {
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia))
                return new HttpUnauthorizedResult();

            // Check permission.
            var rootMediaFolder = _mediaLibraryService.GetRootMediaFolder();
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent) && !_mediaLibraryService.CanManageMediaFolder(folderPath)) {
                return new HttpUnauthorizedResult();
            }

            var statuses = new List<object>();

            // Loop through each file in the request
            for (int i = 0; i < HttpContext.Request.Files.Count; i++) {
                // Pointer to file
                var file = HttpContext.Request.Files[i];
                var filename = Path.GetFileName(file.FileName);
                
                // if the file has been pasted, provide a default name
                if (file.ContentType.Equals("image/png", StringComparison.InvariantCultureIgnoreCase) && !filename.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)) {
                    filename = "clipboard.png";
                }

                var mediaPart = _mediaLibraryService.ImportMedia(file.InputStream, folderPath, filename, type);
                _contentManager.Create(mediaPart);

                statuses.Add(new {
                    id = mediaPart.Id,
                    name = mediaPart.Title,
                    type = mediaPart.MimeType,
                    size = file.ContentLength,
                    progress = 1.0,
                    url= mediaPart.FileName,
                });
            }

            // Return JSON
            return Json(statuses, JsonRequestBehavior.AllowGet);
        }
    }
}