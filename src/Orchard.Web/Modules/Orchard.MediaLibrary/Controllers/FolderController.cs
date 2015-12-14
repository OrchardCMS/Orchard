using System;
using System.IO;
using System.Linq;
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
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia, T("Couldn't create media folder")))
                return new HttpUnauthorizedResult();

            // If the user is trying to access a folder above his boundaries, redirect him to his home folder
            var rootMediaFolder = _mediaLibraryService.GetRootMediaFolder();
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent) && !_mediaLibraryService.CanManageMediaFolder(folderPath)) {
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
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia, T("Couldn't create media folder")))
                return new HttpUnauthorizedResult();

            var viewModel = new MediaManagerFolderCreateViewModel();
            UpdateModel(viewModel);

            if (!_mediaLibraryService.CanManageMediaFolder(viewModel.FolderPath)) {
                return new HttpUnauthorizedResult();
            }

            try {
                _mediaLibraryService.CreateFolder(viewModel.FolderPath, viewModel.Name);
                Services.Notifier.Information(T("Media folder created"));
            }
            catch (ArgumentException argumentException) {
                Services.Notifier.Error(T("Creating Folder failed: {0}", argumentException.Message));
                Services.TransactionManager.Cancel();
                return View(viewModel);
            }

            return RedirectToAction("Index", "Admin", new { area = "Orchard.MediaLibrary" });

        }

        public ActionResult Edit(string folderPath) {
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia, T("Couldn't edit media folder")))
                return new HttpUnauthorizedResult();

            if (!_mediaLibraryService.CanManageMediaFolder(folderPath)) {
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
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia, T("Couldn't edit media folder")))
                return new HttpUnauthorizedResult();

            var viewModel = new MediaManagerFolderEditViewModel();
            UpdateModel(viewModel);

            if (!_mediaLibraryService.CanManageMediaFolder(viewModel.FolderPath)) {
                return new HttpUnauthorizedResult();
            }

            try {
                _mediaLibraryService.RenameFolder(viewModel.FolderPath, viewModel.Name);
                Services.Notifier.Information(T("Media folder renamed"));
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
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia, T("Couldn't delete media folder")))
                return new HttpUnauthorizedResult();

            var viewModel = new MediaManagerFolderEditViewModel();
            UpdateModel(viewModel);

            if (!_mediaLibraryService.CanManageMediaFolder(viewModel.FolderPath)) {
                return new HttpUnauthorizedResult();

            }
            try {
                _mediaLibraryService.DeleteFolder(viewModel.FolderPath);
                Services.Notifier.Information(T("Media folder deleted"));
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
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia, T("Couldn't move media items")))
                return new HttpUnauthorizedResult();

            if (!_mediaLibraryService.CanManageMediaFolder(folderPath)) {
                return new HttpUnauthorizedResult();
            }

            foreach (var media in Services.ContentManager.Query().ForPart<MediaPart>().ForContentItems(mediaItemIds).List()) {

                // don't try to rename the file if there is no associated media file
                if (!String.IsNullOrEmpty(media.FileName)) {
                    var uniqueFilename = _mediaLibraryService.GetUniqueFilename(folderPath, media.FileName);
                    _mediaLibraryService.MoveFile(media.FolderPath, media.FileName, folderPath, uniqueFilename);
                    media.FileName = uniqueFilename;
                }

                media.FolderPath = folderPath;
            }

            return Json(true);
        }
    }
}