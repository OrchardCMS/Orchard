using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
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

        public ActionResult Index(int id) {

            return View(id);
        }


        [HttpPost]
        public JsonResult ImagePost(int id, string url) {

            try {
                var buffer = new WebClient().DownloadData(url);
                var stream = new MemoryStream(buffer);
                var mediaPart = _mediaLibraryService.ImportStream(id, stream, Path.GetFileName(url));

                return new JsonResult {
                    Data = new {id, mediaPart.Resource}
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