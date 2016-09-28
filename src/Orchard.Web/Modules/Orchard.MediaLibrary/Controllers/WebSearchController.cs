using System;
using System.IO;
using System.Net;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.MediaLibrary.Services;
using Orchard.MediaLibrary.ViewModels;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.FileSystems.Media;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Controllers {
    [Admin, Themed(false)]
    public class WebSearchController : Controller {
        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly IContentManager _contentManager;
        private readonly IMimeTypeProvider _mimeTypeProvider;
        
        public WebSearchController(
            IMediaLibraryService mediaManagerService, 
            IContentManager contentManager,
            IOrchardServices orchardServices,
            IMimeTypeProvider mimeTypeProvider) {
            _mediaLibraryService = mediaManagerService;
            _contentManager = contentManager;
            _mimeTypeProvider = mimeTypeProvider;

            Services = orchardServices;
        }

        public IOrchardServices Services { get; set; }

        public ActionResult Index(string folderPath, string type, int? replaceId = null) {
            var viewModel = new ImportMediaViewModel {
                FolderPath = folderPath,
                Type = type,
                ReplaceId = replaceId
            };

            return View(viewModel);
        }


        [HttpPost]
        public ActionResult ImagePost(string folderPath, string type, string url) {
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia))
                return new HttpUnauthorizedResult();

            // Check permission.
            var rootMediaFolder = _mediaLibraryService.GetRootMediaFolder();
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent) && !_mediaLibraryService.CanManageMediaFolder(folderPath)) {
                return new HttpUnauthorizedResult();
            }

            try {
                var buffer = new WebClient().DownloadData(url);
                var stream = new MemoryStream(buffer);
                
                var mediaPart = _mediaLibraryService.ImportMedia(stream, folderPath, Path.GetFileName(url), type);
                _contentManager.Create(mediaPart);

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

        [HttpPost]
        public ActionResult ReplacePost(string folderPath, string type, string url, int? replaceId = null) {
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia))
                return new HttpUnauthorizedResult();

            var rootMediaFolder = _mediaLibraryService.GetRootMediaFolder();
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent) && !_mediaLibraryService.CanManageMediaFolder(folderPath)) {
                return new HttpUnauthorizedResult();
            }

            try {
                if (replaceId != null) {
                    var replaceItem = Services.ContentManager.Get(replaceId.Value).As<MediaPart>();
                    folderPath = replaceItem.FolderPath;
                    var replaceItemType = replaceItem.TypeDefinition.Name;
                    if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent) && !_mediaLibraryService.CanManageMediaFolder(folderPath)) {
                        return new HttpUnauthorizedResult();
                    }
                    var buffer = new WebClient().DownloadData(url);
                    var stream = new MemoryStream(buffer);
                    var mimeType = _mimeTypeProvider.GetMimeType(Path.GetFileName(url));
                    var mediaFactory = _mediaLibraryService.GetMediaFactory(stream, mimeType, "");
                    if (replaceItemType.Equals(mediaFactory.GetContentType(type),StringComparison.OrdinalIgnoreCase)) {
                        _mediaLibraryService.DeleteFile(replaceItem.FolderPath, replaceItem.FileName);
                        _mediaLibraryService.UploadMediaFile(replaceItem.FolderPath, replaceItem.FileName, stream);
                        _contentManager.Publish(replaceItem.ContentItem);
                        return new JsonResult {
                            Data = new { folderPath, MediaPath = replaceItem.FileName, Success = true }
                        };
                    }

                    return new JsonResult {
                        Data = new { Success = false, error = string.Format("Cannot replace {0} with {1}", replaceItemType, mediaFactory.GetContentType(type)) }
                    };
                }

                return new JsonResult {
                    Data = new { Success = false, error = "replaceId not specified" }
                };
            }
            catch (Exception e) {
                return new JsonResult
                {
                    Data = new { Success = false, error = e.Message }
                };
            }
        }
    }
}