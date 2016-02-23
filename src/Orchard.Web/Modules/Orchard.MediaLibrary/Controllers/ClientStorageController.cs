using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.MediaLibrary.Services;
using Orchard.MediaLibrary.ViewModels;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.MediaLibrary.Models;
using Orchard.Localization;
using System.Linq;

namespace Orchard.MediaLibrary.Controllers {
    [Admin, Themed(false)]
    public class ClientStorageController : Controller {
        private readonly IMediaLibraryService _mediaLibraryService;

        public ClientStorageController(
            IMediaLibraryService mediaManagerService, 
            IContentManager contentManager,
            IOrchardServices orchardServices) {
            _mediaLibraryService = mediaManagerService;
            Services = orchardServices;
            Services = orchardServices;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index(string folderPath, string type) {
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia)) {
                return new HttpUnauthorizedResult();
            }
            
            // Check permission.
            var rootMediaFolder = _mediaLibraryService.GetRootMediaFolder();

            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent) && !_mediaLibraryService.CanManageMediaFolder(folderPath)) {
                return new HttpUnauthorizedResult();
            }

            var viewModel = new ImportMediaViewModel {
                FolderPath = folderPath,
                Type = type
            };

            return View(viewModel);
        }
        
        [HttpPost]
        public ActionResult Upload(string folderPath, string type) {
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia)) { 
                return new HttpUnauthorizedResult();
            }

            // Check permission.
            var rootMediaFolder = _mediaLibraryService.GetRootMediaFolder();
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent) && !_mediaLibraryService.CanManageMediaFolder(folderPath)) {
                return new HttpUnauthorizedResult();
            }

            var statuses = new List<object>();
            var settings = Services.WorkContext.CurrentSite.As<MediaLibrarySettingsPart>();
            var allowedExtensions = (settings.UploadAllowedFileTypeWhitelist ?? "")
                .Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x.StartsWith("."));

            // Loop through each file in the request
            for (int i = 0; i < HttpContext.Request.Files.Count; i++) {
                // Pointer to file
                var file = HttpContext.Request.Files[i];
                var filename = Path.GetFileName(file.FileName);
                
                // if the file has been pasted, provide a default name
                if (file.ContentType.Equals("image/png", StringComparison.InvariantCultureIgnoreCase) && !filename.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)) {
                    filename = "clipboard.png";
                }

                // skip file if the allowed extensions is defined and doesn't match
                if(allowedExtensions.Any()) {
                    if(!allowedExtensions.Any(e => filename.EndsWith(e, StringComparison.OrdinalIgnoreCase))) {
                        statuses.Add(new {
                            error = T("This file type is not allowed: {0}", Path.GetExtension(filename)).Text,
                            progress = 1.0,
                        });
                        continue;
                    }
                }

                var mediaPart = _mediaLibraryService.ImportMedia(file.InputStream, folderPath, filename, type);
                Services.ContentManager.Create(mediaPart);

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