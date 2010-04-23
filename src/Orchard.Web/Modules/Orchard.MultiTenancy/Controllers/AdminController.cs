using System;
using System.Web.Mvc;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.MultiTenancy.Services;
using Orchard.MultiTenancy.ViewModels;
using Orchard.UI.Notify;

namespace Orchard.MultiTenancy.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly ITenantService _tenantService;

        public AdminController(ITenantService tenantService, IOrchardServices orchardServices) {
            _tenantService = tenantService;
            Services = orchardServices;
            T = NullLocalizer.Instance;
        }

        private Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        public ActionResult Index() {
            return View(new TenantsIndexViewModel { TenantSettings = _tenantService.GetTenants() });
        }

        public ActionResult Add() {
            return View(new TenantsAddViewModel());
        }

        [HttpPost, ActionName("Add")]
        public ActionResult AddPOST(TenantsAddViewModel viewModel) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ManageTenants, T("Couldn't create tenant")))
                    return new HttpUnauthorizedResult();
                _tenantService.CreateTenant(
                    new ShellSettings {
                        Name = viewModel.Name,
                        RequestUrlHost = viewModel.RequestUrlHost,
                        RequestUrlPrefix = viewModel.RequestUrlPrefix,
                        State = new TenantState("Uninitialized")
                    });

                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Creating Tenant failed: ") + exception.Message);
                return View(viewModel);
            }
        }
    }
}