using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Orchard.MediaLibrary.Services;
using Orchard.Themes;
using Orchard.UI.Admin;

namespace Orchard.MediaLibrary.Controllers {
    [Admin, Themed(false)]
    public class ClientStorageController : Controller {
        private readonly IMediaLibraryService _mediaLibraryService;

        public ClientStorageController(IMediaLibraryService mediaManagerService) {
            _mediaLibraryService = mediaManagerService;
        }

        public ActionResult Index(string folderPath) {

            return View((object)folderPath);
        }
        
        [HttpPost]
        public ActionResult Upload(string folderPath) {
            var statuses = new List<object>();

            // Loop through each file in the request
            for (int i = 0; i < HttpContext.Request.Files.Count; i++) {
                // Pointer to file
                var file = HttpContext.Request.Files[i];
                var filename = Path.GetFileName(file.FileName);
                
                // if the file has been pasted, provide a default name
                if (filename == "blob") {
                    filename = "clipboard.png";
                }

                var mediaPart = _mediaLibraryService.ImportMedia(file.InputStream, folderPath, filename);

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