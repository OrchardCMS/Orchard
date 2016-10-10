﻿using System;
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
using Orchard.FileSystems.Media;

namespace Orchard.MediaLibrary.Controllers {
    [Admin, Themed(false)]
    public class ClientStorageController : Controller {
        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly IMimeTypeProvider _mimeTypeProvider;

        public ClientStorageController(
            IMediaLibraryService mediaManagerService,
            IOrchardServices orchardServices,
            IMimeTypeProvider mimeTypeProvider) {
            _mediaLibraryService = mediaManagerService;
            _mimeTypeProvider = mimeTypeProvider;
            Services = orchardServices;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index(string folderPath, string type, int? replaceId = null) {
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia)) {
                return new HttpUnauthorizedResult();
            }

            // Check permission
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent) && !_mediaLibraryService.CanManageMediaFolder(folderPath)) {
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
        public ActionResult Upload(string folderPath, string type) {
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia)) {
                return new HttpUnauthorizedResult();
            }

            // Check permission
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent) && !_mediaLibraryService.CanManageMediaFolder(folderPath)) {
                return new HttpUnauthorizedResult();
            }

            var statuses = new List<object>();
            var settings = Services.WorkContext.CurrentSite.As<MediaLibrarySettingsPart>();
            var allowedExtensions = (settings.UploadAllowedFileTypeWhitelist ?? "")
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
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
                if (allowedExtensions.Any()) {
                    if (!allowedExtensions.Any(e => filename.EndsWith(e, StringComparison.OrdinalIgnoreCase))) {
                        statuses.Add(new {
                            error = T("This file type is not allowed: {0}", Path.GetExtension(filename)).Text,
                            progress = 1.0,
                        });
                        continue;
                    }
                }

                try {
                    var mediaPart = _mediaLibraryService.ImportMedia(file.InputStream, folderPath, filename, type);
                    Services.ContentManager.Create(mediaPart);

                    statuses.Add(new {
                        id = mediaPart.Id,
                        name = mediaPart.Title,
                        type = mediaPart.MimeType,
                        size = file.ContentLength,
                        progress = 1.0,
                        url = mediaPart.FileName,
                    });
                }
                catch (Exception ex) {
                    statuses.Add(new {
                        error = T(ex.Message).Text,
                        progress = 1.0,
                    });
                }
            }

            // Return JSON
            return Json(statuses, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Replace(int replaceId, string type) {
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia))
                return new HttpUnauthorizedResult();

            var replaceMedia = Services.ContentManager.Get<MediaPart>(replaceId);
            if (replaceMedia == null)
                return HttpNotFound();

            // Check permission
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent) && !_mediaLibraryService.CanManageMediaFolder(replaceMedia.FolderPath)) {
                return new HttpUnauthorizedResult();
            }

            var statuses = new List<object>();

            var settings = Services.WorkContext.CurrentSite.As<MediaLibrarySettingsPart>();
            var allowedExtensions = (settings.UploadAllowedFileTypeWhitelist ?? "")
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
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
                if (allowedExtensions.Any()) {
                    if (!allowedExtensions.Any(e => filename.EndsWith(e, StringComparison.OrdinalIgnoreCase))) {
                        statuses.Add(new {
                            error = T("This file type is not allowed: {0}", Path.GetExtension(filename)).Text,
                            progress = 1.0,
                        });
                        continue;
                    }
                }

                try {
                    var mimeType = _mimeTypeProvider.GetMimeType(filename);

                    string replaceContentType = _mediaLibraryService.MimeTypeToContentType(file.InputStream, mimeType, type) ?? type;
                    if (!replaceContentType.Equals(replaceMedia.TypeDefinition.Name, StringComparison.OrdinalIgnoreCase))
                        throw new Exception(T("Cannot replace {0} with {1}", replaceMedia.TypeDefinition.Name, replaceContentType).Text);

                    _mediaLibraryService.DeleteFile(replaceMedia.FolderPath, replaceMedia.FileName);
                    _mediaLibraryService.UploadMediaFile(replaceMedia.FolderPath, replaceMedia.FileName, file.InputStream);
                    replaceMedia.MimeType = mimeType;

                    // Force a publish event which will update relevant Media properties
                    replaceMedia.ContentItem.VersionRecord.Published = false;
                    Services.ContentManager.Publish(replaceMedia.ContentItem);

                    statuses.Add(new {
                        id = replaceMedia.Id,
                        name = replaceMedia.Title,
                        type = replaceMedia.MimeType,
                        size = file.ContentLength,
                        progress = 1.0,
                        url = replaceMedia.FileName,
                    });
                }
                catch (Exception ex) {
                    statuses.Add(new {
                        error = T(ex.Message).Text,
                        progress = 1.0,
                    });
                }
            }

            return Json(statuses, JsonRequestBehavior.AllowGet);
        }
    }
}