using System;
using System.IO;
using System.Net;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Services;
using Orchard.MediaLibrary.ViewModels;
using Orchard.Themes;
using Orchard.UI.Admin;

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
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index(string folderPath, string type, int? replaceId = null) {
            if (!_mediaLibraryService.CheckMediaFolderPermission(Permissions.SelectMediaContent, folderPath)) {
                return new HttpUnauthorizedResult();
            }

            // Check permission
            if (!_mediaLibraryService.CanManageMediaFolder(folderPath)) {
                return new HttpUnauthorizedResult();
            }

            var viewModel = new ImportMediaViewModel {
                FolderPath = folderPath,
                Type = type,
            };

            if (replaceId != null) {
                var replaceMedia = Services.ContentManager.Get<MediaPart>(replaceId.Value);
                if (replaceMedia == null)
                    return HttpNotFound();

                viewModel.Replace = replaceMedia;
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Import(string folderPath, string type, string url) {
            if (!_mediaLibraryService.CheckMediaFolderPermission(Permissions.ImportMediaContent, folderPath))
                return new HttpUnauthorizedResult();

            // Check permission
            if (!_mediaLibraryService.CanManageMediaFolder(folderPath)) {
                return new HttpUnauthorizedResult();
            }

            var settings = Services.WorkContext.CurrentSite.As<MediaLibrarySettingsPart>();

            try {
                var filename = Path.GetFileName(url);

                // skip file if the allowed extensions is defined and doesn't match
                if (!settings.IsFileAllowed(filename)) {
                    throw new Exception(T("This file is not allowed: {0}", filename).Text);
                }

                var buffer = new WebClient().DownloadData(url);
                var stream = new MemoryStream(buffer);

                var mediaPart = _mediaLibraryService.ImportMedia(stream, folderPath, filename, type);
                _contentManager.Create(mediaPart);

                return new JsonResult { Data = new { folderPath, MediaPath = mediaPart.FileName } };

            }
            catch (Exception e) {
                return new JsonResult { Data = new { error = e.Message } };
            }
        }

        [HttpPost]
        public ActionResult Replace(int replaceId, string type, string url) {
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia))
                return new HttpUnauthorizedResult();

            var replaceMedia = Services.ContentManager.Get<MediaPart>(replaceId);
            if (replaceMedia == null)
                return HttpNotFound();

            // Check permission
            if (!(_mediaLibraryService.CheckMediaFolderPermission(Permissions.EditMediaContent, replaceMedia.FolderPath) && _mediaLibraryService.CheckMediaFolderPermission(Permissions.ImportMediaContent, replaceMedia.FolderPath)) 
                && !_mediaLibraryService.CanManageMediaFolder(replaceMedia.FolderPath)) {
                return new HttpUnauthorizedResult();
            }

            var settings = Services.WorkContext.CurrentSite.As<MediaLibrarySettingsPart>();

            try {
                var filename = Path.GetFileName(url);

                // skip file if the allowed extensions is defined and doesn't match
                if (!settings.IsFileAllowed(filename)) {
                    throw new Exception(T("This file is not allowed: {0}", filename).Text);
                }

                var buffer = new WebClient().DownloadData(url);
                var stream = new MemoryStream(buffer);

                var mimeType = _mimeTypeProvider.GetMimeType(filename);

                string replaceContentType = _mediaLibraryService.MimeTypeToContentType(stream, mimeType, type) ?? type;
                if (!replaceContentType.Equals(replaceMedia.TypeDefinition.Name, StringComparison.OrdinalIgnoreCase))
                    throw new Exception(T("Cannot replace {0} with {1}", replaceMedia.TypeDefinition.Name, replaceContentType).Text);

                _mediaLibraryService.DeleteFile(replaceMedia.FolderPath, replaceMedia.FileName);
                _mediaLibraryService.UploadMediaFile(replaceMedia.FolderPath, replaceMedia.FileName, stream);
                replaceMedia.MimeType = mimeType;

                // Force a publish event which will update relevant Media properties
                replaceMedia.ContentItem.VersionRecord.Published = false;
                Services.ContentManager.Publish(replaceMedia.ContentItem);

                return new JsonResult { Data = new { replaceMedia.FolderPath, MediaPath = replaceMedia.FileName } };
            }
            catch (Exception e) {
                return new JsonResult { Data = new { Success = false, error = e.Message } };
            }
        }
    }
}