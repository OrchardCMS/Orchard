using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Orchard.Media.Models;
using Orchard.Media.Services;
using Orchard.Media.ViewModels;
using Orchard.Security;
using Orchard.UI.Notify;

namespace Orchard.Media.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly IMediaService _mediaService;
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;

        public AdminController(IMediaService mediaService, IAuthorizationService authorizationService, INotifier notifier) {
            _mediaService = mediaService;
            _authorizationService = authorizationService;
            _notifier = notifier;
        }

        // That's what it takes to get the CurrentUser for a module.
        // See Orchard.Security.SecurityModule.
        public IUser CurrentUser { get; set; }

        public ActionResult Index() {
            // Root media folders
            IEnumerable<MediaFolder> mediaFolders = _mediaService.GetMediaFolders(null);
            var model = new MediaFolderIndexViewModel { MediaFolders = mediaFolders };
            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Index(FormCollection input) {
            try {
                foreach (string key in input.Keys) {
                    if (key.StartsWith("Checkbox.") && input[key] == "true") {
                        string folderName = key.Substring("Checkbox.".Length);
                        _mediaService.DeleteFolder(folderName);
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                _notifier.Error("Deleting Folder failed: " + exception.Message);
                return View();
            }
        }

        public ActionResult Create(string mediaPath) {
            return View(new MediaFolderCreateViewModel { MediaPath = mediaPath });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create(FormCollection input) {
            var viewModel = new MediaFolderCreateViewModel();
            try {
                UpdateModel(viewModel, input.ToValueProvider());
                if (!_authorizationService.CheckAccess(CurrentUser, Permissions.CreateMediaFolderPermission)) {
                    _notifier.Error("Couldn't create media folder, user " + 
                        (CurrentUser != null ? CurrentUser.UserName : String.Empty) + " doesn't have " + 
                        Permissions.CreateMediaFolderPermission.Name);
                    //return new HttpUnauthorizedResult();
                    return View(viewModel);
                }
                _mediaService.CreateFolder(viewModel.MediaPath, viewModel.Name);
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                _notifier.Error("Creating Folder failed: " + exception.Message);
                return View(viewModel);
            }
        }

        public ActionResult Edit(string name, string mediaPath) {
            IEnumerable<MediaFile> mediaFiles = _mediaService.GetMediaFiles(mediaPath);
            IEnumerable<MediaFolder> mediaFolders = _mediaService.GetMediaFolders(mediaPath);
            var model = new MediaFolderEditViewModel { FolderName = name, MediaFiles = mediaFiles, MediaFolders = mediaFolders, MediaPath = mediaPath };
            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Edit(FormCollection input) {
            try {
                foreach (string key in input.Keys) {
                    if (key.StartsWith("Checkbox.File.") && input[key] == "true") {
                        string fileName = key.Substring("Checkbox.File.".Length);
                        string folderName = input[fileName];
                        if (!_authorizationService.CheckAccess(CurrentUser, Permissions.DeleteMediaPermission)) {
                            _notifier.Error("Couldn't delete media file, user " +
                                (CurrentUser != null ? CurrentUser.UserName : String.Empty) + " doesn't have " +
                                Permissions.DeleteMediaPermission.Name);
                            //return new HttpUnauthorizedResult();
                            return RedirectToAction("Index");
                        }
                        _mediaService.DeleteFile(fileName, folderName);
                    }
                    else if (key.StartsWith("Checkbox.Folder.") && input[key] == "true") {
                        string folderName = key.Substring("Checkbox.Folder.".Length);
                        string folderPath = input[folderName];
                        if (!_authorizationService.CheckAccess(CurrentUser, Permissions.DeleteMediaFolderPermission)) {
                            _notifier.Error("Couldn't delete media folder, user " +
                                (CurrentUser != null ? CurrentUser.UserName : String.Empty) + " doesn't have " +
                                Permissions.DeleteMediaFolderPermission.Name);
                            //return new HttpUnauthorizedResult();
                            return RedirectToAction("Index");
                        }
                        _mediaService.DeleteFolder(folderPath);
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                _notifier.Error("Deleting failed: " + exception.Message);
                return View();
            }
        }

        public ActionResult EditProperties(string folderName, string mediaPath) {
            var model = new MediaFolderEditPropertiesViewModel { Name = folderName, MediaPath = mediaPath };
            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditProperties(FormCollection input) {
            var viewModel = new MediaFolderEditPropertiesViewModel();
            try {
                UpdateModel(viewModel, input.ToValueProvider());
                //TODO: There may be better ways to do this.
                // Delete
                if (!String.IsNullOrEmpty(HttpContext.Request.Form["submit.Delete"])) {
                    if (!_authorizationService.CheckAccess(CurrentUser, Permissions.DeleteMediaFolderPermission)) {
                        _notifier.Error("Couldn't delete media folder, user " + 
                            (CurrentUser != null ? CurrentUser.UserName : String.Empty) + " doesn't have " + 
                            Permissions.DeleteMediaFolderPermission.Name);
                        //return new HttpUnauthorizedResult();
                        return View(viewModel);
                    }
                    _mediaService.DeleteFolder(viewModel.MediaPath);
                }
                // Save
                else {
                    if (!_authorizationService.CheckAccess(CurrentUser, Permissions.RenameMediaFolderPermission)) {
                        _notifier.Error("Couldn't rename media folder, user " + 
                            (CurrentUser != null ? CurrentUser.UserName : String.Empty) + " doesn't have " + 
                            Permissions.RenameMediaFolderPermission.Name);
                        //return new HttpUnauthorizedResult();
                        return View(viewModel);
                    }
                    _mediaService.RenameFolder(viewModel.MediaPath, viewModel.Name);
                }

                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                _notifier.Error("Modifying Folder Properties failed: " + exception.Message);
                return View(viewModel);
            }
        }

        public ActionResult Add(string folderName, string mediaPath) {
            var model = new MediaItemAddViewModel { FolderName = folderName, MediaPath = mediaPath };
            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Add(FormCollection input) {
            var viewModel = new MediaItemAddViewModel();
            try {
                UpdateModel(viewModel, input.ToValueProvider());

                if (!_authorizationService.CheckAccess(CurrentUser, Permissions.UploadMediaPermission)) {
                    _notifier.Error("Couldn't upload media file, user " + 
                        (CurrentUser != null ? CurrentUser.UserName : String.Empty) + " doesn't have " +
                        Permissions.UploadMediaPermission.Name);
                    //return new HttpUnauthorizedResult();
                    return View(viewModel);
                }

                foreach (string fileName in Request.Files) {
                    HttpPostedFileBase file = Request.Files[fileName];
                    _mediaService.UploadMediaFile(viewModel.MediaPath, file);
                }

                return RedirectToAction("Edit", new { name = viewModel.FolderName, mediaPath = viewModel.MediaPath });
            }
            catch (Exception exception) {
                _notifier.Error("Uploading media file failed: " + exception.Message);
                return View(viewModel);
            }
        }

        public ActionResult EditMedia(string name, string caption, DateTime lastUpdated, long size, string folderName, string mediaPath) {
            var model = new MediaItemEditViewModel();
            model.Name = name;
            model.Caption = caption ?? String.Empty;
            model.LastUpdated = lastUpdated;
            model.Size = size;
            model.FolderName = folderName;
            model.MediaPath = mediaPath;
            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditMedia(FormCollection input) {
            var viewModel = new MediaItemEditViewModel();
            try {
                UpdateModel(viewModel, input.ToValueProvider());
                if (!_authorizationService.CheckAccess(CurrentUser, Permissions.ModifyMediaPermission)) {
                    _notifier.Error("Couldn't modify media file, user " + 
                        (CurrentUser != null ? CurrentUser.UserName : String.Empty) + " doesn't have " +
                        Permissions.ModifyMediaPermission.Name);
                    //return new HttpUnauthorizedResult();
                    return View(viewModel);
                }
                //TODO: There may be better ways to do this.
                // Delete
                if (!String.IsNullOrEmpty(HttpContext.Request.Form["submit.Delete"])) {
                    if (!_authorizationService.CheckAccess(CurrentUser, Permissions.DeleteMediaPermission)) {
                        _notifier.Error("Couldn't delete media file, user " + 
                            (CurrentUser != null ? CurrentUser.UserName : String.Empty) + " doesn't have " + 
                            Permissions.DeleteMediaPermission.Name);
                        //return new HttpUnauthorizedResult();
                        return View(viewModel);
                    }
                    _mediaService.DeleteFile(viewModel.Name, viewModel.MediaPath);
                    return RedirectToAction("Edit", new { name = viewModel.FolderName, mediaPath = viewModel.MediaPath });
                }
                // Save and Rename
                if (!String.Equals(viewModel.Name, input["NewName"], StringComparison.OrdinalIgnoreCase)) {
                    _mediaService.RenameFile(viewModel.Name, input["NewName"], viewModel.MediaPath);
                    return RedirectToAction("EditMedia", new { name = input["NewName"],
                                                               caption = viewModel.Caption,
                                                               lastUpdated = viewModel.LastUpdated,
                                                               size = viewModel.Size,
                                                               folderName = viewModel.FolderName,
                                                               mediaPath = viewModel.MediaPath });
                }
                // Save
                return RedirectToAction("EditMedia", new { name = viewModel.Name, 
                                                           caption = viewModel.Caption, 
                                                           lastUpdated = viewModel.LastUpdated, 
                                                           size = viewModel.Size,
                                                           folderName = viewModel.FolderName,
                                                           mediaPath = viewModel.MediaPath });
            }
            catch (Exception exception) {
                _notifier.Error("Editing media file failed: " + exception.Message);
                return View(viewModel);
            }
        }
    }
}
