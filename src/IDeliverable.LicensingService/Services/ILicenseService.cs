using IDeliverable.LicensingService.Models;

namespace IDeliverable.LicensingService.Services
{
    public interface ILicenseService
    {
        LicenseValidationToken ValidateLicense(int productId, string hostname, string key);
    }
}