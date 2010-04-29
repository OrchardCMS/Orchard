using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Modules.ViewModels;
using Orchard.Mvc.AntiForgery;
using Orchard.Mvc.Results;
using Orchard.UI.Notify;

namespace Orchard.Modules.Controllers {
    public class AdminController : Controller {
        private readonly IModuleService _moduleService;

        public AdminController(IOrchardServices services, IModuleService moduleService) {
            Services = services;
            _moduleService = moduleService;
            T = NullLocalizer.Instance;
        }

        private Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        public ActionResult Index() {
            if (!Services.Authorizer.Authorize(Permissions.ManageModules, T("Not allowed to manage modules")))
                return new HttpUnauthorizedResult();

            var modules = _moduleService.GetInstalledModules();
            return View(new ModulesIndexViewModel {Modules = modules});
        }

        public ActionResult Edit(string moduleName) {
            if (!Services.Authorizer.Authorize(Permissions.ManageModules, T("Not allowed to edit module")))
                return new HttpUnauthorizedResult();

            var module = _moduleService.GetModuleByName(moduleName);

            if (module == null)
                return new NotFoundResult();

            return View(new ModuleEditViewModel {
                                               Name = module.DisplayName
                                           });
        }

        public ActionResult Features(FeaturesOptions options) {
            if (!Services.Authorizer.Authorize(Permissions.ManageFeatures, T("Not allowed to manage features")))
                return new HttpUnauthorizedResult();

            var features = _moduleService.GetAvailableFeatures();
            return View(new FeaturesViewModel {Features = features, Options = options});
        }

        [HttpPost, ActionName("Features")]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult FeaturesPOST(FeaturesOptions options, IList<string> selection) {
            if (selection != null && selection.Count > 0)
            {
                switch (options.BulkAction)
                {
                    case FeaturesBulkAction.None:
                        break;
                    case FeaturesBulkAction.Enable:
                        if (!Services.Authorizer.Authorize(Permissions.ManageFeatures, T("Not allowed to enable features")))
                            return new HttpUnauthorizedResult();
                        _moduleService.EnableFeatures(selection);
                        //todo: (heskew) need better messages
                        //todo: (heskew) hmmm...need a helper to comma-separate all but last, which would get the " and " treatment...all localized, of course
                        Services.Notifier.Information(T("{0} were enabled", string.Join(", ", selection.ToArray())));
                        break;
                    case FeaturesBulkAction.Disable:
                        if (!Services.Authorizer.Authorize(Permissions.ManageFeatures, T("Not allowed to disable features")))
                            return new HttpUnauthorizedResult();
                        _moduleService.DisableFeatures(selection);
                        //todo: (heskew) need better messages
                        Services.Notifier.Information(T("{0} were disabled", string.Join(", ", selection.ToArray())));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return RedirectToAction("Features");
        }

        [ValidateAntiForgeryTokenOrchard]
        public ActionResult Enable(string featureName) {
            if (!Services.Authorizer.Authorize(Permissions.ManageFeatures, T("Not allowed to manage features")))
                return new HttpUnauthorizedResult();

            if (string.IsNullOrEmpty(featureName))
                return new NotFoundResult();

            _moduleService.EnableFeatures(new [] {featureName});
            Services.Notifier.Information(T("{0} was enabled", featureName));

            return RedirectToAction("Features");
        }

        [ValidateAntiForgeryTokenOrchard]
        public ActionResult Disable(string featureName) {
            if (!Services.Authorizer.Authorize(Permissions.ManageFeatures, T("Not allowed to manage features")))
                return new HttpUnauthorizedResult();

            if (string.IsNullOrEmpty(featureName))
                return new NotFoundResult();

            _moduleService.DisableFeatures(new[] { featureName });
            Services.Notifier.Information(T("{0} was disabled", featureName));

            return RedirectToAction("Features");
        }

        private class FormValueRequiredAttribute : ActionMethodSelectorAttribute {
            private readonly string _submitButtonName;

            public FormValueRequiredAttribute(string submitButtonName) {
                _submitButtonName = submitButtonName;
            }

            public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo) {
                var value = controllerContext.HttpContext.Request.Form[_submitButtonName];
                return !string.IsNullOrEmpty(value);
            }
        }
    }
}