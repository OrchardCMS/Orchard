using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
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

        [HttpGet]
        public HttpResponseMessage Test()
        {
            var token = Validate(Int32.Parse(ConfigurationManager.AppSettings["TestProductId"]), ConfigurationManager.AppSettings["TestHostname"], ConfigurationManager.AppSettings["TestKey"]);

            if (token.Error.HasValue)
                return new HttpResponseMessage(HttpStatusCode.InternalServerError) { ReasonPhrase = $"Validation for a supposedly valid license returned error code '{token.Error}'." };

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
