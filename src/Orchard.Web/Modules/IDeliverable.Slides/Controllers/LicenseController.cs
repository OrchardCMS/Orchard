using IDeliverable.Licensing;
using IDeliverable.Licensing.Orchard;
using IDeliverable.Slides.Services;
using Orchard;

namespace IDeliverable.Slides.Controllers
{
    public class LicenseController : LicenseControllerBase
    {
        private IProductManifestManager _productManifestManager;

        public LicenseController(
            IOrchardServices services, 
            ILicenseValidator licenseValidator, 
            ILicenseAccessor licenseAccessor,
            IProductManifestManager productManifestManager) : 
            base(services, licenseValidator, licenseAccessor)
        {
            _productManifestManager = productManifestManager;
        }

        protected override ProductManifest ProductManifest => SlidesProductManifest.ProductManifest;
    }
}