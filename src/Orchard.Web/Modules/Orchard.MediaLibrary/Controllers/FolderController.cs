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
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent, T("Couldn't create media folder")))
                return new HttpUnauthorizedResult();

            var viewModel = new MediaManagerFolderCreateViewModel {
                Hierarchy = _mediaLibraryService.GetMediaFolders(folderPath),
                FolderPath = folderPath
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult Create() {
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent, T("Couldn't create media folder")))
                return new HttpUnauthorizedResult();

            var viewModel = new MediaManagerFolderCreateViewModel();
            UpdateModel(viewModel);

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
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent, T("Couldn't edit media folder")))
                return new HttpUnauthorizedResult();
            
            var viewModel = new MediaManagerFolderEditViewModel {
                FolderPath = folderPath,
                Name = folderPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Last()
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public ActionResult Edit() {
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent, T("Couldn't edit media folder")))
                return new HttpUnauthorizedResult();

            var viewModel = new MediaManagerFolderEditViewModel();
            UpdateModel(viewModel);

            try {
                _mediaLibraryService.RenameFolder(viewModel.FolderPath, viewModel.Name);

                var segments = viewModel.FolderPath.Split(new char[] {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries).ToArray();
                var newFolderPath = String.Join(Path.DirectorySeparatorChar.ToString(), segments.Take(segments.Length - 1).Union(new [] { viewModel.Name }));

                foreach (var media in Services.ContentManager.Query().ForPart<MediaPart>().Where<MediaPartRecord>(m => m.FolderPath.StartsWith(viewModel.FolderPath)).List()) {
                    media.FolderPath = newFolderPath + media.FolderPath.Substring(viewModel.FolderPath.Length);
                }
                Services.Notifier.Information(T("Media folder renamed"));
            }
            catch (ArgumentException argumentException) {
                Services.Notifier.Error(T("Editing Folder failed: {0}", argumentException.Message));
                Services.TransactionManager.Cancel();
                return View(viewModel);
            }

            return RedirectToAction("Index", "Admin", new { area = "Orchard.MediaLibrary" });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Delete")]
        public ActionResult Delete() {
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent, T("Couldn't delete media folder")))
                return new HttpUnauthorizedResult();

            var viewModel = new MediaManagerFolderEditViewModel();
            UpdateModel(viewModel);

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
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent, T("Couldn't move media items")))
                return new HttpUnauthorizedResult();

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