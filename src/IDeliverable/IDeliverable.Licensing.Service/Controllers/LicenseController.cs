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
            var sendOwlApiEndpoint = ConfigurationManager.AppSettings["SendOwlApiEndpoint"];
            var sendOwlApiKey = ConfigurationManager.AppSettings["SendOwlApiKey"];
            var sendOwlApiSecret = ConfigurationManager.AppSettings["SendOwlApiSecret"];
            var tokenSigningCertificateThumbprint = ConfigurationManager.AppSettings["TokenSigningCertificateThumbprint"];

            var service = new LicenseService(sendOwlApiEndpoint, sendOwlApiKey, sendOwlApiSecret, tokenSigningCertificateThumbprint);

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
            var testProductId = ConfigurationManager.AppSettings["TestProductId"];
            var testHostname = ConfigurationManager.AppSettings["TestHostname"];
            var testKey = ConfigurationManager.AppSettings["TestKey"];

            var token = Validate(Int32.Parse(testProductId), testHostname, testKey);

            if (token.Error.HasValue)
                return new HttpResponseMessage(HttpStatusCode.InternalServerError) { ReasonPhrase = $"Validation for a supposedly valid license returned error code '{token.Error}'." };

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
