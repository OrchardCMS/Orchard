using System.Web.Http;
using IDeliverable.Licensing.Service.Services;

namespace IDeliverable.Licensing.Service.Controllers
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
