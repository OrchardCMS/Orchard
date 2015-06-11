using Orchard;

namespace IDeliverable.Licensing.Orchard
{
    public interface ILicenseAccessor : IDependency
    {
        ILicense GetLicense(ProductManifest productManifest);
        void UpdateLicense(ProductManifest productManifest, string key);
        LicenseValidationToken GetLicenseValidationToken(ProductManifest productManifest);
        void UpdateLicenseValidationToken(LicenseValidationToken token);
    }
}