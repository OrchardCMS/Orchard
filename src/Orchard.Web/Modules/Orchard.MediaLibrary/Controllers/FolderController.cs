using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Services;
using Orchard.MediaLibrary.ViewModels;
using Orchard.Mvc;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Orchard.MediaLibrary.Controllers {
    [Admin]
    public class FolderController : Controller {
        private readonly IMediaLibraryService _mediaLibraryService;

        public FolderController(
            IOrchardServices services,
            IMediaLibraryService mediaManagerService
            ) {
            _mediaLibraryService = mediaManagerService;

            Services = services;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public ActionResult Create(string folderPath) {
            if (!(_mediaLibraryService.CheckMediaFolderPermission(Permissions.ImportMediaContent, folderPath) || _mediaLibraryService.CheckMediaFolderPermission(Permissions.EditMediaContent, folderPath))) {
                Services.Notifier.Error(T("Couldn't create media folder"));
                return RedirectToAction("Index", "Admin", new { area = "Orchard.MediaLibrary", folderPath = folderPath });
            }

            // If the user is trying to access a folder above his boundaries, redirect him to his home folder
            var rootMediaFolder = _mediaLibraryService.GetRootMediaFolder();
            if (!_mediaLibraryService.CanManageMediaFolder(folderPath)) {
                return RedirectToAction("Create", new { folderPath = rootMediaFolder.MediaPath });
            }

            var viewModel = new MediaManagerFolderCreateViewModel {
                Hierarchy = _mediaLibraryService.GetMediaFolders(folderPath),
                FolderPath = folderPath
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult Create() {
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia)) {
                Services.Notifier.Error(T("Couldn't create media folder"));
                return new HttpUnauthorizedResult();
            }

            var viewModel = new MediaManagerFolderCreateViewModel();
            UpdateModel(viewModel);

            if (!(_mediaLibraryService.CheckMediaFolderPermission(Permissions.ImportMediaContent, viewModel.FolderPath)
                || _mediaLibraryService.CheckMediaFolderPermission(Permissions.EditMediaContent, viewModel.FolderPath))) {
                return new HttpUnauthorizedResult();
            }

            try {
                bool valid = String.IsNullOrWhiteSpace(viewModel.Name) || Regex.IsMatch(viewModel.Name, @"^[^:?#\[\]@!$&'()*+,.;=\s\""\<\>\\\|%]+$");
                if (!valid) {
                    throw new ArgumentException(T("Folder contains invalid characters").ToString());
                }
                else {
                    _mediaLibraryService.CreateFolder(viewModel.FolderPath, viewModel.Name);
                    Services.Notifier.Information(T("Media folder created"));
                }
            }
            catch (ArgumentException argumentException) {
                Services.Notifier.Error(T("Creating Folder failed: {0}", argumentException.Message));
                Services.TransactionManager.Cancel();
                return View(viewModel);
            }
            return RedirectToAction("Index", "Admin", new { area = "Orchard.MediaLibrary" });
        }

        public ActionResult Edit(string folderPath) {
            if (!(_mediaLibraryService.CheckMediaFolderPermission(Permissions.ImportMediaContent, folderPath) || _mediaLibraryService.CheckMediaFolderPermission(Permissions.EditMediaContent, folderPath))) {
                Services.Notifier.Error(T("Couldn't edit media folder"));
                return RedirectToAction("Index", "Admin", new { area = "Orchard.MediaLibrary", folderPath = folderPath });
            }

            if (!_mediaLibraryService.CanManageMediaFolder(folderPath)) {
                return new HttpUnauthorizedResult();
            }

            // Shouldn't be able to rename the root folder
            if (IsRootFolder(folderPath)) {
                return new HttpUnauthorizedResult();
            }

            // Shouldn't be able to rename Users folder
            if (folderPath == "Users") {
                return new HttpUnauthorizedResult();
            }

            var viewModel = new MediaManagerFolderEditViewModel {
                FolderPath = folderPath,
                Name = folderPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Last()
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public ActionResult Edit() {
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia)) {
                Services.Notifier.Error(T("Couldn't edit media folder"));
                return new HttpUnauthorizedResult();
            }

            var viewModel = new MediaManagerFolderEditViewModel();
            UpdateModel(viewModel);

            if (!(_mediaLibraryService.CheckMediaFolderPermission(Permissions.ImportMediaContent, viewModel.FolderPath) 
                || _mediaLibraryService.CheckMediaFolderPermission(Permissions.EditMediaContent, viewModel.FolderPath))) {
                return new HttpUnauthorizedResult();
            }

            // Shouldn't be able to rename the root folder
            if (IsRootFolder(viewModel.FolderPath)) {
                return new HttpUnauthorizedResult();
            }

            try {
                bool valid = String.IsNullOrWhiteSpace(viewModel.Name) || Regex.IsMatch(viewModel.Name, @"^[^:?#\[\]@!$&'()*+,.;=\s\""\<\>\\\|%]+$");
                if (!valid) {
                    throw new ArgumentException(T("Folder contains invalid characters").ToString());
                }
                else {
                    _mediaLibraryService.RenameFolder(viewModel.FolderPath, viewModel.Name);
                    Services.Notifier.Information(T("Media folder renamed"));
                }
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Editing Folder failed: {0}", exception.Message));
                return View(viewModel);
            }

            return RedirectToAction("Index", "Admin", new { area = "Orchard.MediaLibrary" });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Delete")]
        public ActionResult Delete() {
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia)) {
                Services.Notifier.Error(T("Couldn't delete media folder"));
                return new HttpUnauthorizedResult();
            }

            var viewModel = new MediaManagerFolderEditViewModel();
            UpdateModel(viewModel);

            if (!_mediaLibraryService.CheckMediaFolderPermission(Permissions.DeleteMediaContent, viewModel.FolderPath)) {
                return new HttpUnauthorizedResult();
            }

            try {
                _mediaLibraryService.DeleteFolder(viewModel.FolderPath);
                Services.Notifier.Success(T("Media folder deleted"));
            }
            catch (ArgumentException argumentException) {
                Services.Notifier.Error(T("Deleting Folder failed: {0}", argumentException.Message));
                Services.TransactionManager.Cancel();
                return View(viewModel);
            }

            return RedirectToAction("Index", "Admin", new { area = "Orchard.MediaLibrary" });
        }

        [HttpPost]
        public ActionResult Move(string folderPath, int[] mediaItemIds) {
            // check permission on destination folder
            if (!_mediaLibraryService.CheckMediaFolderPermission(Permissions.ImportMediaContent, folderPath)) {
                Services.Notifier.Error(T("Couldn't move media items"));
                return new HttpUnauthorizedResult();
            }

            if (!_mediaLibraryService.CanManageMediaFolder(folderPath)) {
                return new HttpUnauthorizedResult();
            }

            foreach (var media in Services.ContentManager.Query().ForPart<MediaPart>().ForContentItems(mediaItemIds).List()) {

                // don't try to rename the file if there is no associated media file
                if (!string.IsNullOrEmpty(media.FileName)) {
                    // check permission on source folder
                    if(!_mediaLibraryService.CheckMediaFolderPermission(Permissions.DeleteMediaContent, media.FolderPath)) {
                        return new HttpUnauthorizedResult();
                    }
                    var uniqueFilename = _mediaLibraryService.GetUniqueFilename(folderPath, media.FileName);
                    _mediaLibraryService.MoveFile(media.FolderPath, media.FileName, folderPath, uniqueFilename);
                    media.FileName = uniqueFilename;
                }

                media.FolderPath = folderPath;
            }

            return Json(true);
        }

        private bool IsRootFolder(string folderPath) {
            var rootMediaFolder = _mediaLibraryService.GetRootMediaFolder();

            return rootMediaFolder == null ?
                string.IsNullOrEmpty(folderPath) :
                string.Equals(rootMediaFolder.MediaPath, folderPath, StringComparison.OrdinalIgnoreCase);
        }
    }
}