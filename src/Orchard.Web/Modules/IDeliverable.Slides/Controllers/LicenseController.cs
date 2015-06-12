using IDeliverable.Licensing.Orchard.Controllers;
using IDeliverable.Licensing.Orchard.Models;
using IDeliverable.Slides.Services;
using Orchard;

namespace IDeliverable.Slides.Controllers
{
    public class LicenseController : LicenseControllerBase
    {
        public LicenseController(IOrchardServices services) : base(services)
        {
        }

        protected override ProductManifest ProductManifest => SlidesProductManifestProvider.ProductManifest;
    }
}