using IDeliverable.Licensing;
using IDeliverable.Licensing.Orchard;
using IDeliverable.Slides.Services;

namespace IDeliverable.Slides.Helpers
{
    public static class LicenseAccessorExtensions
    {
        public static ILicense GetSlidesLicense(this ILicenseAccessor licenseAccessor)
        {
            return licenseAccessor.GetLicense(SlidesProductManifest.ProductManifest);
        }
    }
}