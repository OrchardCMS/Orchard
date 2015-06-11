using System;
using System.Web.Mvc;
using IDeliverable.Licensing;
using Orchard;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;

namespace IDeliverable.Slides.Controllers
{
    [Admin]
    public abstract class LicenseControllerBase : Controller
    {
        protected  LicenseControllerBase(IOrchardServices services, ILicenseValidator licenseValidator, ILicenseAccessor licenseAccessor)
        {
            Services = services;
            LicenseValidator = licenseValidator;
            LicenseAccessor = licenseAccessor;
            T = NullLocalizer.Instance;
        }

        protected abstract ProductManifest ProductManifest { get; }
        protected IOrchardServices Services { get; private set; }
        protected ILicenseValidator LicenseValidator { get; private set; }
        protected ILicenseAccessor LicenseAccessor { get; private set; }

        public Localizer T { get; set; }

        public ActionResult Index()
        {
            var license = LicenseAccessor.GetLicense(ProductManifest);
            var licenseValidationResult = !String.IsNullOrWhiteSpace(license.Key) ? LicenseValidator.ValidateLicense(license, LicenseValidationOptions.RefreshToken | LicenseValidationOptions.SkipLocalRequests) : default(LicenseValidationResult);
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

            LicenseAccessor.UpdateLicense(ProductManifest, viewModel.Key?.Trim());
            return RedirectToAction("Index");
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner))
                filterContext.Result = new HttpUnauthorizedResult();
        }
    }
}