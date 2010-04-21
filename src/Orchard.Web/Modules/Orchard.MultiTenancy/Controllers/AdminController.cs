using System.Web.Mvc;
using Orchard.Localization;
using Orchard.MultiTenancy.Services;
using Orchard.MultiTenancy.ViewModels;

namespace Orchard.MultiTenancy.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly ITenantService _tenantService;

        public AdminController(ITenantService tenantService) {
            _tenantService = tenantService;
            T = NullLocalizer.Instance;
        }

        private Localizer T { get; set; }

        public ActionResult List() {
            return View(new TenantsListViewModel { TenantSettings = _tenantService.GetTenants() });
        }

        public ActionResult Add() {
            return View(new TenantsAddViewModel());
        }
    }
}