using System;
using System.IO;
using System.Net;
using System.Web.Mvc;
using Orchard.MediaLibrary.Services;
using Orchard.Themes;
using Orchard.UI.Admin;

namespace Orchard.MediaLibrary.Controllers {
    [Admin, Themed(false)]
    public class WebSearchController : Controller {
        private readonly IMediaLibraryService _mediaLibraryService;

        public WebSearchController(IMediaLibraryService mediaManagerService) {
            _mediaLibraryService = mediaManagerService;
        }

        public ActionResult Index(string folderPath) {

            return View((object)folderPath);
        }


        [HttpPost]
        public JsonResult ImagePost(string folderPath, string url) {

            try {
                var buffer = new WebClient().DownloadData(url);
                var stream = new MemoryStream(buffer);
                var mediaPart = _mediaLibraryService.ImportMedia(stream, folderPath, Path.GetFileName(url));

                return new JsonResult {
                    Data = new {folderPath, MediaPath = mediaPart.FileName}
                };
            }
            catch(Exception e) {
                return new JsonResult {
                    Data = new { error= e.Message }
                };
            }
            
        }
    }
}