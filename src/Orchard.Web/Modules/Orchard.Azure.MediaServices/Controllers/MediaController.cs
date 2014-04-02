using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Orchard.Azure.MediaServices.Models;
using Orchard.Azure.MediaServices.Models.Assets;
using Orchard.Azure.MediaServices.Services.Assets;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.MediaLibrary.Models;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Orchard.Azure.MediaServices.Controllers {

    [Admin]
    public class MediaController : Controller, IUpdateModel {

        private readonly IContentManager _contentManager;
        private readonly IAssetManager _assetManager;
        private readonly INotifier _notifier;
        private readonly ITransactionManager _transactionManager;
        private readonly IAuthorizer _authorizer;

        public MediaController(
            IOrchardServices services, 
            IAssetManager assetManager, 
            ITransactionManager transactionManager) {

            _contentManager = services.ContentManager;
            _assetManager = assetManager;
            _transactionManager = transactionManager;
            _authorizer = services.Authorizer;
            _notifier = services.Notifier;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;

            New = services.New;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        private dynamic New { get; set; }

        public ActionResult Import(string folderPath) {
            var part = _contentManager.New<CloudVideoPart>("CloudVideo");
            return EditImplementation(part, folderPath);
        }

        [HttpPost, ActionName("Import")]
        [FormValueRequired("submit.Save")]
        public ActionResult ImportSave(string folderPath) {
            var part = _contentManager.Create<CloudVideoPart>("CloudVideo", VersionOptions.Draft);
            return UpdateImplementation(part, folderPath, T("The cloud video item was successfully created."), publish: false);
        }

        [HttpPost, ActionName("Import")]
        [FormValueRequired("submit.Publish")]
        public ActionResult ImportPublish(string folderPath) {
            var part = _contentManager.Create<CloudVideoPart>("CloudVideo", VersionOptions.Draft);
            return UpdateImplementation(part, folderPath, T("The cloud video item was successfully created."), publish: true);
        }

        public ActionResult Edit(int id) {
            var part = _contentManager.Get<CloudVideoPart>(id, VersionOptions.Latest);
            return EditImplementation(part, null);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditSave(int id) {
            var part = _contentManager.Get<CloudVideoPart>(id, VersionOptions.Latest);
            return UpdateImplementation(part, null, T("The cloud video item was successfully updated."), publish: false);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Publish")]
        public ActionResult EditPublish(int id) {
            var part = _contentManager.Get<CloudVideoPart>(id, VersionOptions.Latest);
            return UpdateImplementation(part, null, T("The cloud video item was successfully updated."), publish: true);
        }

        [HttpPost]
        public ActionResult Upload() {
            if (!_authorizer.Authorize(Permissions.ManageCloudMediaContent, T("You are not authorized to manage Microsoft Azure Media content.")))
                return new HttpUnauthorizedResult();

            if (HttpContext.Request.Files.Count < 1)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "At least one file must be provided in the upload request.");

            var postedFile = HttpContext.Request.Files[0];
            Logger.Debug("User requested asynchronous upload of file with name '{0}' and size {1} bytes to temporary storage.", postedFile.FileName, postedFile.ContentLength);

            var fileName = _assetManager.SaveTemporaryFile(postedFile);
            Logger.Information("File with name '{0}' and size {1} bytes was uploaded to temporary storage.", postedFile.FileName, postedFile.ContentLength);
            return Json(new {
                originalFileName = Path.GetFileName(postedFile.FileName),
                temporaryFileName = fileName,
                fileSize = postedFile.ContentLength
            });
        }

        private ActionResult EditImplementation(IContent content, string folderPath) {
            if (!_authorizer.Authorize(Permissions.ManageCloudMediaContent, T("You are not authorized to manage Microsoft Azure Media content.")))
                return new HttpUnauthorizedResult();

            var editorShape = _contentManager.BuildEditor(content);
            var model = New.ViewModel(Editor: editorShape, FolderPath: folderPath);
            return View(model);
        }

        private ActionResult UpdateImplementation(CloudVideoPart part, string folderPath, LocalizedString notification, bool publish) {
            if (!_authorizer.Authorize(Permissions.ManageCloudMediaContent, T("You are not authorized to manage Microsoft Azure Media content.")))
                return new HttpUnauthorizedResult();

            Logger.Debug("User requested to save cloud video item with ID {0}.", part.Id);

            var editorShape = _contentManager.UpdateEditor(part, this);
            
            if (!ModelState.IsValid) {
                _transactionManager.Cancel();

                var viewModel = New.ViewModel(FolderPath: folderPath, Editor: editorShape);
                return View(viewModel);
            }

            var mediaPart = part.As<MediaPart>();
            mediaPart.LogicalType = "CloudVideo";

            if (String.IsNullOrWhiteSpace(mediaPart.MimeType)) {
                var mezzanineAsset = _assetManager.LoadAssetsFor<MezzanineAsset>(part).Single();
                mediaPart.MimeType = mezzanineAsset.MimeType;
            }

            if (!String.IsNullOrWhiteSpace(folderPath))
                mediaPart.FolderPath = folderPath;

            try {
                if (publish)
                    _contentManager.Publish(mediaPart.ContentItem);

                Logger.Information("Cloud video item with ID {0} was saved.", part.Id);
                _notifier.Information(notification);
            }
            catch (Exception ex) {
                _transactionManager.Cancel();

                Logger.Error(ex, "Error while saving cloud video item with ID {0}.", part.Id);                
                _notifier.Error(T("Ar error occurred while saving the cloud video item:\n{1}", ex.Message));
            }

            return RedirectToAction("Edit", new { id = part.Id });
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}