using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.MultiTenancy.Services;
using Orchard.MultiTenancy.ViewModels;
using Orchard.Security;
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
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index() {
            return View(new TenantsIndexViewModel { TenantSettings = _tenantService.GetTenants() });
        }

        public ActionResult Add() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Cannot create tenant")))
                return new HttpUnauthorizedResult();

            if ( !EnsureDefaultTenant() )
                return new HttpUnauthorizedResult();

            var model = new TenantAddViewModel();

            // fetches all available themes and modules
            model.Themes = _tenantService.GetInstalledThemes().Select(x => new ThemeEntry { ThemeId = x.Id, ThemeName = x.Name }).ToList();
            model.Modules = _tenantService.GetInstalledModules().Select(x => new ModuleEntry { ModuleId = x.Id, ModuleName = x.Name }).ToList();

            return View(model);
        }

        [HttpPost, ActionName("Add")]
        public ActionResult AddPOST(TenantAddViewModel viewModel) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Couldn't create tenant"))) {
                return new HttpUnauthorizedResult();
            }

            if (!EnsureDefaultTenant()) {
                return new HttpUnauthorizedResult();
            }

            if (_tenantService.GetTenants().Any(tenant => string.Equals(tenant.Name, viewModel.Name, StringComparison.OrdinalIgnoreCase))) {
                ModelState.AddModelError("Name", T("A tenant with the same name already exists.", viewModel.Name).Text);
            }

            // ensure tenants name are valid
            if (!String.IsNullOrEmpty(viewModel.Name) && !Regex.IsMatch(viewModel.Name, @"^\w+$")) {
                ModelState.AddModelError("Name", T("Invalid tenant name. Must contain characters only and no spaces.").Text);
            }

            if (!ModelState.IsValid) {
                return View(viewModel);
            }

            try {
                _tenantService.CreateTenant(
                    new ShellSettings {
                        Name = viewModel.Name,
                        RequestUrlHost = viewModel.RequestUrlHost,
                        RequestUrlPrefix = viewModel.RequestUrlPrefix,
                        DataProvider = viewModel.DataProvider,
                        DataConnectionString = viewModel.DatabaseConnectionString,
                        DataTablePrefix = viewModel.DatabaseTablePrefix,
                        State = TenantState.Uninitialized,
                        Themes = viewModel.Themes.Where(x => x.Checked).Select(x => x.ThemeId).ToArray(),
                        Modules = viewModel.Modules.Where(x => x.Checked).Select(x => x.ModuleId).ToArray()
                    });

                return RedirectToAction("Index");
            }
            catch (ArgumentException exception) {
                Services.Notifier.Error(T("Creating Tenant failed: {0}", exception.Message));
                return View(viewModel);
            }
        }

        public ActionResult Edit(string name) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Cannot edit tenant")))
                return new HttpUnauthorizedResult();

            if ( !EnsureDefaultTenant() )
                return new HttpUnauthorizedResult();

            var tenant = _tenantService.GetTenants().FirstOrDefault(ss => ss.Name == name);
            
            if (tenant == null)
                return HttpNotFound();

            return View(new TenantEditViewModel {
                                                    Name = tenant.Name,
                                                    RequestUrlHost = tenant.RequestUrlHost,
                                                    RequestUrlPrefix = tenant.RequestUrlPrefix,
                                                    DataProvider = tenant.DataProvider,
                                                    DatabaseConnectionString = tenant.DataConnectionString,
                                                    DatabaseTablePrefix = tenant.DataTablePrefix,
                                                    State = tenant.State,
                                                    Themes = _tenantService.GetInstalledThemes().Select(x => new ThemeEntry { 
                                                        ThemeId = x.Id, 
                                                        ThemeName = x.Name,
                                                        Checked = tenant.Themes.Contains(x.Id)
                                                    }).ToList(),
                                                    Modules = _tenantService.GetInstalledModules().Select(x => new ModuleEntry {
                                                        ModuleId = x.Id,
                                                        ModuleName = x.Name,
                                                        Checked = tenant.Modules.Contains(x.Id)
                                                    }).ToList()
                                                });
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(TenantEditViewModel viewModel) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Couldn't edit tenant")))
                return new HttpUnauthorizedResult();

            if ( !EnsureDefaultTenant() )
                return new HttpUnauthorizedResult();

            var tenant = _tenantService.GetTenants().FirstOrDefault(ss => ss.Name == viewModel.Name);
            if (tenant == null)
                return HttpNotFound();

            if (!ModelState.IsValid) {
                return View(viewModel);
            }

            try {
                _tenantService.UpdateTenant(
                    new ShellSettings(tenant) {
                        Name = tenant.Name,
                        RequestUrlHost = viewModel.RequestUrlHost,
                        RequestUrlPrefix = viewModel.RequestUrlPrefix,
                        DataProvider = viewModel.DataProvider,
                        DataConnectionString = viewModel.DatabaseConnectionString,
                        DataTablePrefix = viewModel.DatabaseTablePrefix,
                        State = tenant.State,
                        EncryptionAlgorithm = tenant.EncryptionAlgorithm,
                        EncryptionKey = tenant.EncryptionKey,
                        HashAlgorithm = tenant.HashAlgorithm,
                        HashKey = tenant.HashKey,
                        Themes = viewModel.Themes.Where(x => x.Checked).Select(x => x.ThemeId).ToArray(),
                        Modules = viewModel.Modules.Where(x => x.Checked).Select(x => x.ModuleId).ToArray()
                    });

                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Failed to edit tenant: {0} ", exception.Message));
                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult Disable(string name) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Couldn't disable tenant")))
                return new HttpUnauthorizedResult();

            if ( !EnsureDefaultTenant() )
                return new HttpUnauthorizedResult();

            var tenant = _tenantService.GetTenants().FirstOrDefault(ss => ss.Name == name);

            if (tenant != null && tenant.Name != _thisShellSettings.Name) {
                tenant.State = TenantState.Disabled;
                _tenantService.UpdateTenant(tenant);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(string name) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Couldn't enable tenant")))
                return new HttpUnauthorizedResult();

            if ( !EnsureDefaultTenant() )
                return new HttpUnauthorizedResult();

            var tenant = _tenantService.GetTenants().FirstOrDefault(ss => ss.Name == name);

            if (tenant != null && tenant.Name != _thisShellSettings.Name) {
                tenant.State = TenantState.Running;
                _tenantService.UpdateTenant(tenant);
            }

            return RedirectToAction("Index");
        }

        private bool EnsureDefaultTenant() {
            return _thisShellSettings.Name == ShellSettings.DefaultName;
        }
    }
}