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
            return View(new TenantsIndexViewModel {
                TenantSettings = _tenantService.GetTenants()
            });
        }

        public ActionResult Add() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("You don't have permission to create tenants.")))
                return new HttpUnauthorizedResult();

            if (!IsExecutingInDefaultTenant())
                return new HttpUnauthorizedResult();

            var viewModel = new TenantAddViewModel();

            // Fetches all available themes and modules.
            viewModel.Themes = _tenantService.GetInstalledThemes().Select(x => new ThemeEntry { ThemeId = x.Id, ThemeName = x.Name }).ToList();
            viewModel.Modules = _tenantService.GetInstalledModules().Select(x => new ModuleEntry { ModuleId = x.Id, ModuleName = x.Name }).ToList();

            return View(viewModel);
        }

        [HttpPost, ActionName("Add")]
        public ActionResult AddPost(TenantAddViewModel viewModel) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("You don't have permission to create tenants."))) {
                return new HttpUnauthorizedResult();
            }

            if (!IsExecutingInDefaultTenant()) {
                return new HttpUnauthorizedResult();
            }

            if (_tenantService.GetTenants().Any(tenant => string.Equals(tenant.Name, viewModel.Name, StringComparison.OrdinalIgnoreCase))) {
                ModelState.AddModelError("Name", T("A tenant with the same name already exists.", viewModel.Name).Text);
            }

            // Ensure tenants name are valid.
            if (!String.IsNullOrEmpty(viewModel.Name) && !Regex.IsMatch(viewModel.Name, @"^[a-zA-Z]\w*$")) {
                ModelState.AddModelError("Name", T("Invalid tenant name. Must contain characters only and no spaces.").Text);
            }

            if (!string.Equals(viewModel.Name, "default", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace( viewModel.RequestUrlHost) && string.IsNullOrWhiteSpace(viewModel.RequestUrlPrefix)) {
                ModelState.AddModelError("RequestUrlHostRequestUrlPrefix", T("RequestUrlHost and RequestUrlPrefix can not be empty at the same time.").Text);
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

                Services.Notifier.Success(T("Tenant '{0}' was created successfully.", viewModel.Name));
                return RedirectToAction("Index");
            }
            catch (ArgumentException ex) {
                Logger.Error(ex, "Error while creating tenant.");
                Services.Notifier.Error(T("Tenant creation failed with error: {0}.", ex.Message));
                return View(viewModel);
            }
        }

        public ActionResult Edit(string name) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("You don't have permission to edit tenants.")))
                return new HttpUnauthorizedResult();

            if (!IsExecutingInDefaultTenant())
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
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("You don't have permission to edit tenants.")))
                return new HttpUnauthorizedResult();

            if (!IsExecutingInDefaultTenant())
                return new HttpUnauthorizedResult();

            var tenant = _tenantService.GetTenants().FirstOrDefault(ss => ss.Name == viewModel.Name);

            if (tenant == null)
                return HttpNotFound();

            if (!string.Equals(viewModel.Name, "default", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(viewModel.RequestUrlHost) && string.IsNullOrWhiteSpace(viewModel.RequestUrlPrefix)) {
                ModelState.AddModelError("RequestUrlHostRequestUrlPrefix", T("RequestUrlHost and RequestUrlPrefix can not be empty at the same time.").Text);
            }

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
            catch (Exception ex) {
                Logger.Error(ex, "Error while editing tenant.");
                Services.Notifier.Error(T("Failed to edit tenant: {0} ", ex.Message));
                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult Disable(string name) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("You don't have permission to disable tenants.")))
                return new HttpUnauthorizedResult();

            if (!IsExecutingInDefaultTenant())
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
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("You don't have permission to enable tenants.")))
                return new HttpUnauthorizedResult();

            if (!IsExecutingInDefaultTenant())
                return new HttpUnauthorizedResult();

            var tenant = _tenantService.GetTenants().FirstOrDefault(ss => ss.Name == name);

            if (tenant != null && tenant.Name != _thisShellSettings.Name) {
                tenant.State = TenantState.Running;
                _tenantService.UpdateTenant(tenant);
            }

            return RedirectToAction("Index");
        }

        public ActionResult Reset(string name) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("You don't have permission to reset tenants.")))
                return new HttpUnauthorizedResult();

            if (!IsExecutingInDefaultTenant())
                return new HttpUnauthorizedResult();

            var tenant = _tenantService.GetTenants().FirstOrDefault(ss => ss.Name == name);
            if (tenant == null)
                return HttpNotFound();

            return View(new TenantResetViewModel() {
                Name = name,
                DatabaseTableNames = _tenantService.GetTenantDatabaseTableNames(tenant)
            });
        }

        [HttpPost, ActionName("Reset")]
        public ActionResult ResetPost(TenantResetViewModel viewModel) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("You don't have permission to reset tenants.")))
                return new HttpUnauthorizedResult();

            if (!IsExecutingInDefaultTenant())
                return new HttpUnauthorizedResult();

            var tenant = _tenantService.GetTenants().FirstOrDefault(ss => ss.Name == viewModel.Name);
            if (tenant == null)
                return HttpNotFound();
            else if (tenant.Name == _thisShellSettings.Name)
                return new HttpUnauthorizedResult();

            if (!ModelState.IsValid) {
                viewModel.DatabaseTableNames = _tenantService.GetTenantDatabaseTableNames(tenant);
                return View(viewModel);
            }

            try {
                _tenantService.ResetTenant(tenant, viewModel.DropDatabaseTables, force: false);
                return RedirectToAction("Index");
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error while resetting tenant.");
                Services.Notifier.Error(T("Failed to reset tenant: {0} ", ex.Message));
                viewModel.DatabaseTableNames = _tenantService.GetTenantDatabaseTableNames(tenant);
                return View(viewModel);
            }
        }

        private bool IsExecutingInDefaultTenant() {
            return _thisShellSettings.Name == ShellSettings.DefaultName;
        }
    }
}
