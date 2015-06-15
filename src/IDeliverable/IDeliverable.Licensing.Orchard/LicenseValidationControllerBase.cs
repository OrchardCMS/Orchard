using System.Web.Mvc;
using Orchard.Localization;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;

namespace IDeliverable.Licensing.Orchard
{
    public abstract class LicenseValidationControllerBase : Controller
    {
        protected LicenseValidationControllerBase(INotifier notifier)
        {
            Notifier = notifier;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        protected INotifier Notifier { get; }
        protected abstract string ProductId { get; }

        public ActionResult VerifyLicenseKey()
        {
            LicenseValidationHelper.Instance.ClearLicenseValidationResult(ProductId);

            if (LicenseValidationHelper.GetLicenseIsValid(ProductId))
                Notifier.Information(T("License key succesfully validated."));

            var urlReferrer = Request.UrlReferrer?.ToString();
            return Redirect(Request.IsLocalUrl(urlReferrer) ? urlReferrer : "~/");
        }
    }
}