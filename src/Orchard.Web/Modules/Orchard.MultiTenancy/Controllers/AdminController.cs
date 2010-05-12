using System;
using System.Linq;
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
        private readonly ShellSettings _thisShellSettings;

        public AdminController(ITenantService tenantService, IOrchardServices orchardServices, ShellSettings shellSettings) {
            _tenantService = tenantService;
            _thisShellSettings = shellSettings;
            
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
                        DataProvider = viewModel.DatabaseOptions != null ? ((bool)viewModel.DatabaseOptions ? "SQLite" : "SqlServer") : null,
                        DataConnectionString = viewModel.DatabaseConnectionString,
                        DataTablePrefix = viewModel.DatabaseTablePrefix,
                        State = new TenantState("Uninitialized")
                    });

                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Creating Tenant failed: ") + exception.Message);
                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult Disable(ShellSettings shellSettings) {
            if (!Services.Authorizer.Authorize(Permissions.ManageTenants, T("Couldn't disable tenant")))
                return new HttpUnauthorizedResult();

            var tenant = _tenantService.GetTenants().FirstOrDefault(ss => ss.Name == shellSettings.Name);

            if (tenant != null && tenant.Name != _thisShellSettings.Name) {
                tenant.State.CurrentState = TenantState.State.Disabled;
                _tenantService.UpdateTenant(tenant);
            }

            return RedirectToAction("index");
        }

        [HttpPost]
        public ActionResult Enable(ShellSettings shellSettings) {
            if (!Services.Authorizer.Authorize(Permissions.ManageTenants, T("Couldn't enable tenant")))
                return new HttpUnauthorizedResult();

            var tenant = _tenantService.GetTenants().FirstOrDefault(ss => ss.Name == shellSettings.Name);

            if (tenant != null && tenant.Name != _thisShellSettings.Name) {
                tenant.State.CurrentState = TenantState.State.Running;
                _tenantService.UpdateTenant(tenant);
            }

            return RedirectToAction("index");
        }
    }
}