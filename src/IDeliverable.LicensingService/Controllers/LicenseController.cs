using System.Web.Http;
using IDeliverable.LicensingService.Exceptions;
using IDeliverable.LicensingService.Models;
using IDeliverable.LicensingService.Services;

namespace IDeliverable.LicensingService.Controllers
{
    public class LicenseController : ApiController
    {
        [HttpGet]
        public LicenseValidationToken Validate(int productId, string hostname, string key)
        {
            var service = new LicenseService();

            try
            {
                return service.ValidateLicense(productId, hostname, key);
            }
            catch (LicenseValidationException ex)
            {
                return LicenseValidationToken.CreateInvalidLicenseToken(ex.Error);
            }
        }        
    }
}
