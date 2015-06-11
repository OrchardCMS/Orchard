using Orchard;

namespace IDeliverable.Licensing
{
    public interface ILicenseValidator : IDependency
    {
        /// <summary>
        /// Returns a value indicating wether the specified license information is valid or not.
        /// </summary>
        LicenseValidationResult ValidateLicense(ILicense license, LicenseValidationOptions options = LicenseValidationOptions.Default);

        /// <summary>
        /// Returns a value indicating wether the specified license information is valid or not.
        /// </summary>
        LicenseValidationResult ValidateLicense(int productId, string key, LicenseValidationOptions options = LicenseValidationOptions.Default);
    }
}