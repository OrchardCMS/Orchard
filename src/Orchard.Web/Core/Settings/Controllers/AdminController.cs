using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Settings.ViewModels;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.Localization.Services;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Notify;

namespace Orchard.Core.Settings.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly ISiteService _siteService;
        private readonly ICultureManager _cultureManager;
        public IOrchardServices Services { get; private set; }

        public AdminController(
            ISiteService siteService,
            IOrchardServices services,
            ICultureManager cultureManager) {
            _siteService = siteService;
            _cultureManager = cultureManager;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index(string groupInfoId) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            dynamic model;
            var site = _siteService.GetSiteSettings();
            if (!string.IsNullOrWhiteSpace(groupInfoId)) {
                model = Services.ContentManager.BuildEditor(site, groupInfoId);

                if (model == null)
                    return HttpNotFound();

                var groupInfo = Services.ContentManager.GetEditorGroupInfo(site, groupInfoId);
                if (groupInfo == null)
                    return HttpNotFound();

                model.GroupInfo = groupInfo;
            }
            else {
                model = Services.ContentManager.BuildEditor(site);
            }

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST(string groupInfoId) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            var site = _siteService.GetSiteSettings();
            dynamic model = Services.ContentManager.UpdateEditor(site, this, groupInfoId);

            GroupInfo groupInfo = null;

            if (!string.IsNullOrWhiteSpace(groupInfoId)) {
                if (model == null) {
                    Services.TransactionManager.Cancel();
                    return HttpNotFound();
                }

                groupInfo = Services.ContentManager.GetEditorGroupInfo(site, groupInfoId);
                if (groupInfo == null) {
                    Services.TransactionManager.Cancel();
                    return HttpNotFound();
                }
            }

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                model.GroupInfo = groupInfo;

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.Notifier.Information(T("Settings updated"));
            return RedirectToAction("Index");
        }

        public ActionResult Culture() {
            //todo: class and/or method attributes for our auth?
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            var model = new SiteCulturesViewModel {
                CurrentCulture = _cultureManager.GetCurrentCulture(HttpContext),
                SiteCultures = _cultureManager.ListCultures(),
            };
            model.AvailableSystemCultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(ci => ci.Name)
                .Where(s => !model.SiteCultures.Contains(s));

            return View(model);
        }

        [HttpPost]
        public ActionResult AddCulture(string systemCultureName, string cultureName) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            cultureName = string.IsNullOrWhiteSpace(cultureName) ? systemCultureName : cultureName;

            if (!string.IsNullOrWhiteSpace(cultureName)) {
                _cultureManager.AddCulture(cultureName);
            }
            return RedirectToAction("Culture");
        }

        [HttpPost]
        public ActionResult DeleteCulture(string cultureName) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage settings")))
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
