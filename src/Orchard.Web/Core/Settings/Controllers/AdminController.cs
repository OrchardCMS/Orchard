using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Orchard.Core.Settings.Models;
using Orchard.Core.Settings.ViewModels;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.Localization.Services;
using Orchard.Settings;
using Orchard.UI.Notify;

namespace Orchard.Core.Settings.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly ISiteService _siteService;
        private readonly ICultureManager _cultureManager;
        public IOrchardServices Services { get; private set; }

        public AdminController(ISiteService siteService, IOrchardServices services, ICultureManager cultureManager) {
            _siteService = siteService;
            _cultureManager = cultureManager;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index(string tabName) {
            if (!Services.Authorizer.Authorize(Permissions.ManageSettings, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            var model = new SettingsIndexViewModel {
                Site = _siteService.GetSiteSettings().As<SiteSettingsPart>(),
                SiteCultures = _cultureManager.ListCultures()
            };
            model.ViewModel = Services.ContentManager.BuildEditorShape(model.Site);
            return View(model);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST(string tabName) {
            if (!Services.Authorizer.Authorize(Permissions.ManageSettings, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            var viewModel = new SettingsIndexViewModel { Site = _siteService.GetSiteSettings().As<SiteSettingsPart>() };
            viewModel.ViewModel = Services.ContentManager.UpdateEditorShape(viewModel.Site.ContentItem, this);

            if (!TryUpdateModel(viewModel)) {
                return View(viewModel);
            }

            Services.Notifier.Information(T("Settings updated"));
            return RedirectToAction("Index");
        }

        public ActionResult Culture() {
            //todo: class and/or method attributes for our auth?
            if (!Services.Authorizer.Authorize(Permissions.ManageSettings, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            var viewModel = new SiteCulturesViewModel {
                CurrentCulture = _cultureManager.GetCurrentCulture(HttpContext),
                SiteCultures = _cultureManager.ListCultures(),
            };
            viewModel.AvailableSystemCultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(ci => ci.Name)
                .Where(s => !viewModel.SiteCultures.Contains(s));

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult AddCulture(string cultureName) {
            if (!Services.Authorizer.Authorize(Permissions.ManageSettings, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            _cultureManager.AddCulture(cultureName);
            return RedirectToAction("Culture");
        }

        [HttpPost]
        public ActionResult DeleteCulture(string cultureName) {
            if (!Services.Authorizer.Authorize(Permissions.ManageSettings, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            _cultureManager.DeleteCulture(cultureName);
            return RedirectToAction("Culture");
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
