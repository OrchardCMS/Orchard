using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Orchard.Extensions;
using Orchard.Localization;
using Orchard.Media.Models;
using Orchard.Media.Services;
using Orchard.Media.ViewModels;
using Orchard.UI.Notify;

namespace Orchard.Media.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly IMediaService _mediaService;

        public AdminController(IOrchardServices services, IMediaService mediaService) {
            Services = services;
            _mediaService = mediaService;
        }

        public IOrchardServices Services { get; set;}
        public Localizer T { get; set; }

        public ActionResult Index() {
            // Root media folders
            IEnumerable<MediaFolder> mediaFolders = _mediaService.GetMediaFolders(null);
            var model = new MediaFolderIndexViewModel { MediaFolders = mediaFolders };
            return View(model);
        }

        [HttpPost]
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
                Services.Notifier.Error("Deleting Folder failed: " + exception.Message);
                return View();
            }
        }

        public ActionResult Create(string mediaPath) {
            return View(new MediaFolderCreateViewModel { MediaPath = mediaPath });
        }

        [HttpPost]
        public ActionResult Create() {
            var viewModel = new MediaFolderCreateViewModel();
            try {
                UpdateModel(viewModel);
                if (!Services.Authorizer.Authorize(Permissions.ManageMediaFiles, T("Couldn't create media folder")))
                    return new HttpUnauthorizedResult();
                _mediaService.CreateFolder(viewModel.MediaPath, viewModel.Name);
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                Services.Notifier.Error("Creating Folder failed: " + exception.Message);
                return View(viewModel);
            }
        }

        public ActionResult Edit(string name, string mediaPath) {
            IEnumerable<MediaFile> mediaFiles = _mediaService.GetMediaFiles(mediaPath);
            IEnumerable<MediaFolder> mediaFolders = _mediaService.GetMediaFolders(mediaPath);
            var model = new MediaFolderEditViewModel { FolderName = name, MediaFiles = mediaFiles, MediaFolders = mediaFolders, MediaPath = mediaPath };
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(FormCollection input) {
            try {
                foreach (string key in input.Keys) {
                    if (key.StartsWith("Checkbox.File.") && input[key] == "true") {
                        string fileName = key.Substring("Checkbox.File.".Length);
                        string folderName = input[fileName];
                        if (!Services.Authorizer.Authorize(Permissions.ManageMediaFiles, T("Couldn't delete media file")))
                            return new HttpUnauthorizedResult();
                        _mediaService.DeleteFile(fileName, folderName);
                    }
                    else if (key.StartsWith("Checkbox.Folder.") && input[key] == "true") {
                        string folderName = key.Substring("Checkbox.Folder.".Length);
                        string folderPath = input[folderName];
                        if (!Services.Authorizer.Authorize(Permissions.ManageMediaFiles, T("Couldn't delete media folder")))
                            return new HttpUnauthorizedResult();
                        _mediaService.DeleteFolder(folderPath);
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                Services.Notifier.Error("Deleting failed: " + exception.Message);
                return View();
            }
        }

        public ActionResult EditProperties(string folderName, string mediaPath) {
            var model = new MediaFolderEditPropertiesViewModel { Name = folderName, MediaPath = mediaPath };
            return View(model);
        }

        [HttpPost]
        public ActionResult EditProperties() {
            var viewModel = new MediaFolderEditPropertiesViewModel();
            try {
                UpdateModel(viewModel);
                //TODO: There may be better ways to do this.
                // Delete
                if (!String.IsNullOrEmpty(HttpContext.Request.Form["submit.Delete"])) {
                    if (!Services.Authorizer.Authorize(Permissions.ManageMediaFiles, T("Couldn't delete media folder")))
                        return new HttpUnauthorizedResult();
                    _mediaService.DeleteFolder(viewModel.MediaPath);
                }
                // Save
                else {
                    if (!Services.Authorizer.Authorize(Permissions.ManageMediaFiles, T("Couldn't rename media folder")))
                        return new HttpUnauthorizedResult();
                    _mediaService.RenameFolder(viewModel.MediaPath, viewModel.Name);
                }

                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                Services.Notifier.Error("Modifying Folder Properties failed: " + exception.Message);
                return View(viewModel);
            }
        }

        public ActionResult Add(string folderName, string mediaPath) {
            var model = new MediaItemAddViewModel { FolderName = folderName, MediaPath = mediaPath };
            return View(model);
        }

        [HttpPost]
        public ActionResult Add()
        {
            var viewModel = new MediaItemAddViewModel();
            try
            {
                UpdateModel(viewModel);
                if (!Services.Authorizer.Authorize(Permissions.UploadMediaFiles, T("Couldn't upload media file")))
                    return new HttpUnauthorizedResult();

                if(Request.Files[0].FileName.IsNullOrEmptyTrimmed()) {
                    ModelState.AddModelError("File", "Select a file to upload");
                }

                if (!ModelState.IsValid)
                    return View(viewModel);

                foreach (string fileName in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    _mediaService.UploadMediaFile(viewModel.MediaPath, file);
                }

                return RedirectToAction("Edit", new { name = viewModel.FolderName, mediaPath = viewModel.MediaPath });
            }
            catch (Exception exception)
            {
                Services.Notifier.Error("Uploading media file failed: " + exception.Message);
                return View(viewModel);
            }
        }

        [HttpPost]
        public ContentResult AddFromClient() {
            var viewModel = new MediaItemAddViewModel();
            try {
                UpdateModel(viewModel);
                if (!Services.Authorizer.Authorize(Permissions.UploadMediaFiles))
                    return Content(string.Format("<script type=\"text/javascript\">var result = {{ error: \"{0}\" }};</script>", T("ERROR: You don't have permission to upload media files")));

                if (Request.Files.Count < 1 || Request.Files[0].ContentLength == 0)
                    return Content(string.Format("<script type=\"text/javascript\">var result = {{ error: \"{0}\" }};</script>", T("HEY: You didn't give me a file to upload")));

                try {
                    _mediaService.GetMediaFiles(viewModel.MediaPath);
                }
                catch //media api needs a little work, like everything else of course ;) <- ;) == my stuff included. to clarify I need a way to know if the path exists or have UploadMediaFile create paths as necessary but there isn't the time to hook that up in the near future
                {
                    _mediaService.CreateFolder(viewModel.MediaPath, "");
                }

                var file = Request.Files[0];
                _mediaService.UploadMediaFile(viewModel.MediaPath, file);

                return Content(string.Format("<script type=\"text/javascript\">var result = {{ url: \"{0}\" }};</script>", Path.Combine(_mediaService.GetRootUrl(), string.Format("{0}/{1}", viewModel.MediaPath, Path.GetFileName(file.FileName)).Replace("//", "/")).Replace("\\", "/")));
            }
            catch (Exception exception) {
                return Content(string.Format("<script type=\"text/javascript\">var result = {{ error: \"{0}\" }};</script>", T("ERROR: Uploading media file failed: {0}", exception.Message)));
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

        [HttpPost]
        public ActionResult EditMedia(FormCollection input) {
            var viewModel = new MediaItemEditViewModel();
            try {
                UpdateModel(viewModel);
                if (!Services.Authorizer.Authorize(Permissions.ManageMediaFiles, T("Couldn't modify media file")))
                    return new HttpUnauthorizedResult();
                // Delete
                if (!String.IsNullOrEmpty(HttpContext.Request.Form["submit.Delete"])) {
                    if (!Services.Authorizer.Authorize(Permissions.ManageMediaFiles, T("Couldn't delete media file")))
                        return new HttpUnauthorizedResult();
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
                Services.Notifier.Error("Editing media file failed: " + exception.Message);
                return View(viewModel);
            }
        }
    }
}
