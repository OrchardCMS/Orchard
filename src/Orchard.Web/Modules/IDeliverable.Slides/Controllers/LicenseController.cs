using IDeliverable.Licensing;
using IDeliverable.Licensing.Orchard;
using IDeliverable.Slides.Services;
using Orchard;

namespace IDeliverable.Slides.Controllers
{
    public class LicenseController : LicenseControllerBase
    {
        public LicenseController(
            IOrchardServices services, 
            ILicenseValidator licenseValidator, 
            ILicenseAccessor licenseAccessor) : 
            base(services, licenseValidator, licenseAccessor)
        {
        }

        protected override ProductManifest ProductManifest => SlidesProductManifest.ProductManifest;
    }
}