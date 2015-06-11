namespace IDeliverable.Licensing.Service.Services
{
    public interface ILicenseService
    {
        LicenseValidationToken ValidateLicense(int productId, string hostname, string key);
    }
}