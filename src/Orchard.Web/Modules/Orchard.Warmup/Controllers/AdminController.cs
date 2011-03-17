using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Contents.Controllers;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Warmup.Models;
using Orchard.UI.Notify;
using Orchard.Warmup.Services;

namespace Orchard.Warmup.Controllers {
    public  class AdminController : Controller, IUpdateModel {
        private readonly IWarmupUpdater _warmupUpdater;

        public AdminController(IOrchardServices services, IWarmupUpdater warmupUpdater) {
            _warmupUpdater = warmupUpdater;
            Services = services;

            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            var warmupPart = Services.WorkContext.CurrentSite.As<WarmupSettingsPart>();
            return View(warmupPart);
        }

        [FormValueRequired("submit")]
        [HttpPost, ActionName("Index")]
        public ActionResult IndexPost() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            var warmupPart = Services.WorkContext.CurrentSite.As<WarmupSettingsPart>();

            if(TryUpdateModel(warmupPart)) {
                Services.Notifier.Information(T("Warmup updated successfully."));
            }

            if (warmupPart.Scheduled) {
                if (warmupPart.Delay <= 0) {
                    AddModelError("Delay", T("Delay must be greater than zero."));
                }
            }

            return View(warmupPart);
        }

        [FormValueRequired("submit.Generate")]
        [HttpPost, ActionName("Index")]
        public ActionResult IndexPostGenerate() {
            var result = IndexPost();
            
            if (ModelState.IsValid) {
                _warmupUpdater.Generate();
                Services.Notifier.Information(T("Static pages have been generated."));
            }

            return result;
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        public void AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}