using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Core.Contents.Controllers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Services;
using Orchard.MediaLibrary.ViewModels;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Orchard.MediaLibrary.Controllers {
    [Admin]
    public class FolderController : Controller {
        private readonly IMediaLibraryService _mediaLibraryService;

        public FolderController(IOrchardServices services, IMediaLibraryService mediaManagerService) {
            _mediaLibraryService = mediaManagerService;

            Services = services;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public ActionResult Create(int? id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent, T("Couldn't create media folder")))
                return new HttpUnauthorizedResult();

            var viewModel = new MediaManagerFolderCreateViewModel {
                Hierarchy = id.HasValue ? _mediaLibraryService.GetMediaFolderHierarchy(id.Value) : Enumerable.Empty<MediaFolder>(),
                ParentFolderId = id
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
                _mediaLibraryService.CreateFolder(viewModel.ParentFolderId, viewModel.Name);
                Services.Notifier.Information(T("Media folder created"));
            }
            catch (ArgumentException argumentException) {
                Services.Notifier.Error(T("Creating Folder failed: {0}", argumentException.Message));
                Services.TransactionManager.Cancel();
                return View(viewModel);
            }

            return RedirectToAction("Index", "Admin", new { area = "Orchard.MediaLibrary" });

        }

        public ActionResult Edit(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent, T("Couldn't edit media folder")))
                return new HttpUnauthorizedResult();

            var folder = _mediaLibraryService.GetMediaFolder(id);

            var viewModel = new MediaManagerFolderEditViewModel {
                Hierarchy = _mediaLibraryService.GetMediaFolderHierarchy(id),
                FolderId = id,
                Name = folder.Name
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
                _mediaLibraryService.RenameFolder(viewModel.FolderId, viewModel.Name);
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
                _mediaLibraryService.DeleteFolder(viewModel.FolderId);
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
        public ActionResult Move(int targetId, int[] mediaItemIds) {
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent, T("Couldn't move media items")))
                return new HttpUnauthorizedResult();

            var targetFolder = _mediaLibraryService.GetMediaFolder(targetId);

            if (targetFolder == null) {
                return Json(false);
            }

            if (mediaItemIds == null || mediaItemIds.Length == 0) {
                return Json(false);
            }

            _mediaLibraryService.MoveMedia(targetId, mediaItemIds);

            return Json(true);
        }
    }
}