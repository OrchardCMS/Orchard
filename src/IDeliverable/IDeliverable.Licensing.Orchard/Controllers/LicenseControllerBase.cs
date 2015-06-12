using System.Web.Mvc;
using IDeliverable.Licensing.Orchard.Helpers;
using IDeliverable.Licensing.Orchard.Models;
using IDeliverable.Licensing.Orchard.Services;
using IDeliverable.Licensing.Orchard.ViewModels;
using Orchard;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;

namespace IDeliverable.Licensing.Orchard.Controllers
{
    [Admin]
    public abstract class LicenseControllerBase : Controller
    {
        private readonly ILicenseFileManager _licenseFileManager;

        protected LicenseControllerBase(IOrchardServices services)
        {
            Services = services;
            LicenseValidator = ServiceFactory.Current.Resolve<ILicenseValidator>();
            _licenseFileManager = ServiceFactory.Current.Resolve<ILicenseFileManager>();
            T = NullLocalizer.Instance;
        }

        protected abstract ProductManifest ProductManifest { get; }
        protected IOrchardServices Services { get; }
        protected ILicenseValidator LicenseValidator { get; }

        public Localizer T { get; set; }

        public ActionResult Index()
        {
            var license = LicenseValidator.GetLicense(ProductManifest);
            var licenseValidationResult = LicenseValidator.ValidateLicense(license, Models.LicenseValidationOptions.RefreshToken | Models.LicenseValidationOptions.SkipLocalRequests);
            var viewModel = new LicenseViewModel
            {
                Key = license.Key,
                Hostname = Request.GetHttpHost(),
                IsValid = licenseValidationResult == null || licenseValidationResult.IsValid,
                LicenseValidationResult = licenseValidationResult
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Index(LicenseViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            var file = _licenseFileManager.Load(ProductManifest.ExtensionName);
            file.LicenseKey = viewModel.Key?.Trim();
            _licenseFileManager.Save(file);
            return RedirectToAction("Index");
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner))
                filterContext.Result = new HttpUnauthorizedResult();
        }
    }
}