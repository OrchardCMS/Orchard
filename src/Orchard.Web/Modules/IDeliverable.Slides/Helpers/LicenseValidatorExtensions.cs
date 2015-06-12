using IDeliverable.Licensing.Orchard;
using IDeliverable.Licensing.Orchard.Services;
using IDeliverable.Slides.Services;

namespace IDeliverable.Slides.Helpers
{
    public static class LicenseValidatorExtensions
    {
        public static bool ValidateSlidesLicense(this ILicenseValidator validator)
        {
            return validator.ValidateLicense(SlidesProductManifestProvider.ProductManifest).IsValid;
        }
    }
}