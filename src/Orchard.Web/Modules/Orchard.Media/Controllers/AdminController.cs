using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Contents.Controllers;
using Orchard.Localization;
using Orchard.Logging;
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

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; set;}
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index() {
            // Root media folders
            IEnumerable<MediaFolder> mediaFolders = _mediaService.GetMediaFolders(null);
            var model = new MediaFolderIndexViewModel { MediaFolders = mediaFolders };
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(FormCollection input) {
            foreach (string key in input.Keys) {
                if (key.StartsWith("Checkbox.") && input[key] == "true") {
                    string folderName = key.Substring("Checkbox.".Length);
                    if (!Services.Authorizer.Authorize(Permissions.ManageMedia, T("Couldn't delete media folder")))
                        return new HttpUnauthorizedResult();
                        
                    try {
                        _mediaService.DeleteFolder(folderName);
                    }
                    catch (Exception exception) {
                        Services.Notifier.Error(T("Deleting Folder failed: {0}", exception.Message));
                        return View();
                    }
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult Create(string mediaPath) {
            return View(new MediaFolderCreateViewModel { MediaPath = mediaPath });
        }

        [HttpPost]
        public ActionResult Create() {
            if (!Services.Authorizer.Authorize(Permissions.ManageMedia, T("Couldn't create media folder")))
                return new HttpUnauthorizedResult();

            var viewModel = new MediaFolderCreateViewModel();
            UpdateModel(viewModel);

            try {
                _mediaService.CreateFolder(viewModel.MediaPath, viewModel.Name);
                Services.Notifier.Information(T("Media folder created"));
            }
            catch(ArgumentException argumentException) {
                Services.Notifier.Error(T("Creating Folder failed: {0}", argumentException.Message));
                return View(viewModel);
            }

            return RedirectToAction("Edit", new { viewModel.Name, viewModel.MediaPath });
        }

        public ActionResult Edit(string name, string mediaPath) {
            try {
                IEnumerable<MediaFile> mediaFiles = _mediaService.GetMediaFiles(mediaPath);
                IEnumerable<MediaFolder> mediaFolders = _mediaService.GetMediaFolders(mediaPath);
                var model = new MediaFolderEditViewModel { FolderName = name, MediaFiles = mediaFiles, MediaFolders = mediaFolders, MediaPath = mediaPath };
                return View(model);
            }
            catch(ArgumentException exception) {
                Services.Notifier.Error(T("Editing failed: {0}", exception.Message));
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Edit(FormCollection input, string name, string mediaPath) {
            foreach (string key in input.Keys) {
                if (key.StartsWith("Checkbox.File.") && input[key] == "true") {
                    string fileName = key.Substring("Checkbox.File.".Length);
                    string folderName = input[fileName];
                    if (!Services.Authorizer.Authorize(Permissions.ManageMedia, T("Couldn't delete media file")))
                        return new HttpUnauthorizedResult();

                    try {
                        _mediaService.DeleteFile(folderName, fileName);
                        Services.Notifier.Information(T("Media file deleted"));
                    }
                    catch (ArgumentException argumentException) {
                        Services.Notifier.Error(T("Deleting failed: {0}", argumentException.Message));
                    } 
                }
                else if (key.StartsWith("Checkbox.Folder.") && input[key] == "true") {
                    string folderName = key.Substring("Checkbox.Folder.".Length);
                    string folderPath = input[folderName];
                    if (!Services.Authorizer.Authorize(Permissions.ManageMedia, T("Couldn't delete media folder")))
                        return new HttpUnauthorizedResult();

                    try {
                        _mediaService.DeleteFolder(folderPath);
                        Services.Notifier.Information(T("Media folder deleted"));
                    }
                    catch(ArgumentException argumentException) {
                        Services.Notifier.Error(T("Deleting failed: {0}", argumentException.Message));
                    }
                }
            }

            return RedirectToAction("Edit", new { name, mediaPath });
        }

        public ActionResult EditProperties(string folderName, string mediaPath) {
            var model = new MediaFolderEditPropertiesViewModel { Name = folderName, MediaPath = mediaPath };
            return View(model);
        }

        [HttpPost, ActionName("EditProperties")]
        [FormValueRequired("submit.Delete")]
        public ActionResult EditPropertiesDeletePOST() {
            if (!Services.Authorizer.Authorize(Permissions.ManageMedia, T("Couldn't delete media folder")))
                return new HttpUnauthorizedResult();

            var viewModel = new MediaFolderEditPropertiesViewModel();
            UpdateModel(viewModel);
            try {
                _mediaService.DeleteFolder(viewModel.MediaPath);
                Services.Notifier.Information(T("Media folder deleted"));
            }
            catch(ArgumentException argumentException) {
                Services.Notifier.Error(T("Deleting media folder failed: {0}", argumentException.Message));
                return View(viewModel);
            }

            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("EditProperties")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditProperties() {
            if (!Services.Authorizer.Authorize(Permissions.ManageMedia, T("Couldn't rename media folder")))
                return new HttpUnauthorizedResult();

            var viewModel = new MediaFolderEditPropertiesViewModel();
            UpdateModel(viewModel);
            try {
                _mediaService.RenameFolder(viewModel.MediaPath, viewModel.Name);
                Services.Notifier.Information(T("Media folder properties modified"));
            }
            catch(ArgumentException argumentException) {
                Services.Notifier.Error(T("Modifying media folder properties failed: {0}", argumentException.Message));
                return View(viewModel);
            }

            return RedirectToAction("Index");
        }

        public ActionResult Add(string folderName, string mediaPath) {
            if (!Services.Authorizer.Authorize(Permissions.ManageMedia, T("Couldn't upload media file")))
                return new HttpUnauthorizedResult();
            
            var currentSite = Services.WorkContext.CurrentSite;

            var model = new MediaItemAddViewModel {
                FolderName = folderName, 
                MediaPath = mediaPath,
                AllowedExtensions = currentSite.As<MediaSettingsPart>().UploadAllowedFileTypeWhitelist
            };

            if(currentSite.SuperUser.Equals(Services.WorkContext.CurrentUser.UserName, StringComparison.Ordinal)) {
                model.AllowedExtensions = String.Empty;
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Add() {
            if (!Services.Authorizer.Authorize(Permissions.ManageMedia, T("Couldn't upload media file")))
                return new HttpUnauthorizedResult();

            var viewModel = new MediaItemAddViewModel();

            UpdateModel(viewModel);

            if(String.IsNullOrWhiteSpace(Request.Files[0].FileName)) {
                ModelState.AddModelError("File", T("Select a file to upload").ToString());
            }

            if (!ModelState.IsValid)
                return View(viewModel);

            foreach (string fileName in Request.Files) {
                try {
                    _mediaService.UploadMediaFile(viewModel.MediaPath, Request.Files[fileName], viewModel.ExtractZip);
                }
                catch (ArgumentException e) {
                    Services.Notifier.Error(T("Uploading media file failed: {0}", e.Message));
                    return View(viewModel);   
                }
            }

            Services.Notifier.Information(T("Media file(s) uploaded"));
            return RedirectToAction("Edit", new { name = viewModel.FolderName, mediaPath = viewModel.MediaPath });
        }

        [HttpPost]
        public ContentResult AddFromClient() {
            if (!Services.Authorizer.Authorize(Permissions.ManageMedia))
                return Content(string.Format("<script type=\"text/javascript\">var result = {{ error: \"{0}\" }};</script>", T("ERROR: You don't have permission to upload media files")));

            var viewModel = new MediaItemAddViewModel();
            UpdateModel(viewModel);

            if (Request.Files.Count < 1 || Request.Files[0].ContentLength == 0)
                return Content(string.Format("<script type=\"text/javascript\">var result = {{ error: \"{0}\" }};</script>", T("HEY: You didn't give me a file to upload")));

            try {
                _mediaService.GetMediaFiles(viewModel.MediaPath);
            }
            catch //media api needs a little work, like everything else of course ;) <- ;) == my stuff included. to clarify I need a way to know if the path exists or have UploadMediaFile create paths as necessary but there isn't the time to hook that up in the near future
            {
                _mediaService.CreateFolder("", viewModel.MediaPath);
            }

            var file = Request.Files[0];
                
            try {
                var publicUrl = _mediaService.UploadMediaFile(viewModel.MediaPath, file, viewModel.ExtractZip);
                return Content(string.Format("<script type=\"text/javascript\">var result = {{ url: \"{0}\" }};</script>", publicUrl));
            }
            catch (ArgumentException argumentException) {
                return Content(string.Format("<script type=\"text/javascript\">var result = {{ error: \"{0}\" }};</script>", T("ERROR: Uploading media file failed: {0}", argumentException.Message)));
            }
        }

        public ActionResult EditMedia(MediaItemEditViewModel model) {
            model.PublicUrl = _mediaService.GetMediaPublicUrl(model.MediaPath, model.Name);
            return View(model);
        }

        [HttpPost, ActionName("EditMedia")]
        [FormValueRequired("submit.Delete")]
        public ActionResult EditMediaDeletePOST(FormCollection input) {
            if (!Services.Authorizer.Authorize(Permissions.ManageMedia, T("Couldn't delete media file")))
                return new HttpUnauthorizedResult();

            var viewModel = new MediaItemEditViewModel();
            UpdateModel(viewModel);
            try {
                _mediaService.DeleteFile(viewModel.Name, viewModel.MediaPath);
                Services.Notifier.Information(T("Media deleted"));
            }
            catch (ArgumentException argumentException) {
                Services.Notifier.Error(T("Removing media file failed: {0}", argumentException.Message));
                return View(viewModel);
            }

            return RedirectToAction("Edit", new { name = viewModel.FolderName, mediaPath = viewModel.MediaPath });
        }

        [HttpPost, ActionName("EditMedia")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditMedia(FormCollection input) {
            if (!Services.Authorizer.Authorize(Permissions.ManageMedia, T("Couldn't modify media file")))
                return new HttpUnauthorizedResult();

            var viewModel = new MediaItemEditViewModel();
            UpdateModel(viewModel);
            string viewModelName = viewModel.Name;

            // Rename
            if (!String.Equals(viewModel.Name, input["NewName"], StringComparison.OrdinalIgnoreCase)) {
                try {
                    _mediaService.RenameFile(viewModel.MediaPath, viewModel.Name, input["NewName"]);
                }
                catch (ArgumentException) {
                    Services.Notifier.Error(T("Editing media file failed."));
                    return EditMedia(viewModel);
                }
                viewModelName = input["NewName"];
            }

            Services.Notifier.Information(T("Media information updated"));
            return RedirectToAction("EditMedia", new { name = viewModelName, 
                                                        caption = viewModel.Caption, 
                                                        lastUpdated = viewModel.LastUpdated, 
                                                        size = viewModel.Size,
                                                        folderName = viewModel.FolderName,
                                                        mediaPath = viewModel.MediaPath });
        }
    }
}