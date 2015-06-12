using IDeliverable.Licensing.Orchard.Models;

namespace IDeliverable.Licensing.Orchard.Services
{
    public interface ILicenseValidator
    {
        ILicense GetLicense(ProductManifest product);

        /// <summary>
        /// Returns a value indicating wether the specified license information is valid or not.
        /// </summary>
        LicenseValidationResult ValidateLicense(ProductManifest product, LicenseValidationOptions options = LicenseValidationOptions.Default);

        /// <summary>
        /// Returns a value indicating wether the specified license information is valid or not.
        /// </summary>
        LicenseValidationResult ValidateLicense(ILicense license, LicenseValidationOptions options = LicenseValidationOptions.Default);
    }
}