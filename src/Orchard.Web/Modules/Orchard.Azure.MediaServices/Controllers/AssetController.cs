using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Orchard.Azure.MediaServices.Helpers;
using Orchard.Azure.MediaServices.Infrastructure.Assets;
using Orchard.Azure.MediaServices.Models.Assets;
using Orchard.Azure.MediaServices.Services.Assets;
using Orchard.Azure.MediaServices.Services.Wams;
using Orchard.Azure.MediaServices.ViewModels.Media;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.Html;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using System.Net;

namespace Orchard.Azure.MediaServices.Controllers {

    [Admin]
    public class AssetController : Controller, IUpdateModel {

        private readonly ITransactionManager _transactionManager;
        private readonly IOrchardServices _services;
        private readonly IAssetManager _assetManager;
        private readonly INotifier _notifier;
        private readonly IAuthorizer _authorizer;
        private readonly IWamsClient _wamsClient;
        private readonly Lazy<IEnumerable<IAssetDriver>> _assetDrivers;

        public AssetController(
            ITransactionManager transactionManager,
            IOrchardServices services,
            IAssetManager assetManager,
            IWamsClient wamsClient,
            Lazy<IEnumerable<IAssetDriver>> assetDrivers) {

            _transactionManager = transactionManager;
            _services = services;
            _assetManager = assetManager;
            _wamsClient = wamsClient;
            _assetDrivers = assetDrivers;
            _notifier = services.Notifier;
            _authorizer = services.Authorizer;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        [HttpPost]
        public async Task<ActionResult> GenerateWamsAsset(string fileName) {
            var asset = await _wamsClient.CreateAssetAsync(fileName).ConfigureAwait(continueOnCapturedContext: false);

            return Json(new {
                sasLocator = asset.SasLocator,
                assetId = asset.AssetId
            });
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteWamsAsset(string id) {
            var asset = _wamsClient.GetAssetById(id);
            await _wamsClient.DeleteAssetAsync(asset).ConfigureAwait(continueOnCapturedContext: false);
            return new EmptyResult();
        }

        public JsonResult State(int id) {
            var asset = _assetManager.GetAssetById(id);
            return Json(new {
                uploadState = new {
                    status = asset.UploadState.Status.ToString(),
                    percentComplete = (int?)asset.UploadState.PercentComplete,
                },
                published = asset.VideoPart != null && asset.VideoPart.IsPublished() // VideoPart can potentially be null here if the user deleted the media item, and an AJAX request was still issued.
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(int id) {
            return Validate(id, asset => {
                var viewModel = BuildAssetViewModel(asset, driver => driver.BuildEditor(asset, _services.New));
                return View(viewModel);
            });

        }

        [HttpPost, ActionName("Edit"), FormValueRequired("button.Save")]
        public ActionResult EditSave(int id) {
            return Validate(id, asset => {
                var viewModel = BuildAssetViewModel(asset, driver => driver.UpdateEditor(asset, this, _services.New));

                if (TryUpdateModel(viewModel, null, null, new[] { "Asset", "SpecializedSettingsShapes" })) {
                    asset.Name = viewModel.Name.TrimSafe();
                    asset.Description = viewModel.Description.TrimSafe();
                    asset.IncludeInPlayer = viewModel.IncludeInPlayer;
                    asset.MediaQuery = viewModel.MediaQuery.TrimSafe();
                }

                if (!ModelState.IsValid) {
                    _transactionManager.Cancel();
                    return View(viewModel);
                }

                _notifier.Information(T("The Asset has been saved."));
                return RedirectToAction("Edit", new { id = id });
            });
        }

        [HttpPost, ActionName("Edit"), FormValueRequired("button.Delete")]
        public ActionResult EditDelete(int id) {
            return Delete(id);
        }

        public ActionResult Delete(int id) {
            if (!_authorizer.Authorize(Permissions.ManageCloudMediaContent, T("You are not authorized to manage Microsoft Azure Media content.")))
                return new HttpUnauthorizedResult();

            Logger.Debug("User requested to delete asset with ID {0}.", id);

            var asset = _assetManager.GetAssetById(id);
            if (asset == null) {
                Logger.Warning("User requested to delete asset with ID {0} but no such asset record exists.", id);
                return HttpNotFound(String.Format("No asset with ID {0} was found.", id));
            }

            var cloudVideoPart = asset.VideoPart;

            if (cloudVideoPart.MezzanineAsset.Record.Id == asset.Record.Id) {
                Logger.Warning("User requested to delete asset with ID {0} but it is the mezzanine asset and cannot be deleted.", id);
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, String.Format("Asset with ID {0} is the mezzanine asset and cannot be deleted.", id));
            }

            try {
                _assetManager.DeleteAsset(asset);

                Logger.Information("Asset with ID {0} was deleted.", id);
                _notifier.Information(T("The asset '{0}' was successfully deleted.", asset.Name));
            }
            catch (Exception ex) {
                _transactionManager.Cancel();

                Logger.Error(ex, "Error while deleting asset with ID {0}.", id);
                _notifier.Error(T("Ar error occurred while deleting the asset '{0}':\n{1}", asset.Name, ex.Message));
            }

            return Redirect(Url.ItemEditUrl(cloudVideoPart));
        }

        private ActionResult Validate(int id, Func<Asset, ActionResult> validationSucceeded) {
            if (!_authorizer.Authorize(Permissions.ManageCloudMediaContent, T("You are not authorized to manage Microsoft Azure Media content.")))
                return new HttpUnauthorizedResult();

            var asset = _assetManager.GetAssetById(id);
            return asset == null ? new HttpNotFoundResult() : validationSucceeded(asset);
        }

        private AssetViewModel BuildAssetViewModel(Asset asset, Func<IAssetDriver, IEnumerable<AssetDriverResult>> driverAction) {
            var specializedSettings = _assetDrivers.Value.SelectMany(driverAction);

            return new AssetViewModel {
                Name = asset.Name,
                Description = asset.Description,
                IncludeInPlayer = asset.IncludeInPlayer,
                MediaQuery = asset.MediaQuery,
                Asset = asset,
                SpecializedSettings = specializedSettings.ToArray()
            };
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}