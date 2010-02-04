using System.Web.Mvc;
using Orchard.Core.Settings.Models;
using Orchard.Core.Settings.ViewModels;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.Settings;
using Orchard.UI.Notify;

namespace Orchard.Core.Settings.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly ISiteService _siteService;
        public IOrchardServices Services { get; private set; }

        public AdminController(ISiteService siteService, IOrchardServices services) {
            _siteService = siteService;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index(string tabName) {
            if (!Services.Authorizer.Authorize(Permissions.ManageSettings, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            var model = new SettingsIndexViewModel {
                Site = _siteService.GetSiteSettings().As<SiteSettings>()
            };
            model.ViewModel = Services.ContentManager.BuildEditorModel(model.Site);
            return View(model);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST(string tabName) {
            if (!Services.Authorizer.Authorize(Permissions.ManageSettings, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            var viewModel = new SettingsIndexViewModel { Site = _siteService.GetSiteSettings().As<SiteSettings>() };
            viewModel.ViewModel = Services.ContentManager.UpdateEditorModel(viewModel.Site.ContentItem, this);

            if (!TryUpdateModel(viewModel)) {
                return View(viewModel);
            }

            Services.Notifier.Information(T("Settings updated"));
            return RedirectToAction("Index");
        }


        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
