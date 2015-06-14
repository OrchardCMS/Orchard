using System.Collections.Generic;
using System.Web.Mvc;
using IDeliverable.Licensing.Orchard;

namespace IDeliverable.Slides.Licensing
{
    public class LicenseValidationErrorBanner : LicenseValidationErrorBannerBase
    {
        public LicenseValidationErrorBanner(IEnumerable<ILicensedProductManifest> products, UrlHelper urlHelper)
            :base(products, urlHelper, LicensedProductManifest.ProductId)
        {
        }
    }
}