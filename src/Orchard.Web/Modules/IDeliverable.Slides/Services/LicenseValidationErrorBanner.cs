using System.Web.Mvc;
using IDeliverable.Licensing.Orchard;
using IDeliverable.Slides.Helpers;

namespace IDeliverable.Slides.Services
{
    public class LicenseValidationErrorBanner : LicenseValidationErrorBannerBase
    {
        public LicenseValidationErrorBanner(UrlHelper urlHelper)
            :base("IDeliverable.Slides", urlHelper)
        {
        }

        protected override void EnsureLicenseIsValid()
        {
            LicenseValidationHelper.EnsureLicenseIsValid();
        }
    }
}