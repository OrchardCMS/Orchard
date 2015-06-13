using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using IDeliverable.Licensing.Service.Exceptions;
using IDeliverable.Licensing.Service.Services;
using IDeliverable.Licensing.VerificationTokens;

namespace IDeliverable.Licensing.Service.Controllers
{
    [RoutePrefix("api/v1")]
    public class LicenseController : ApiController
    {
        [HttpGet]
        [Route("license/{licenseKey}/verify")]
        public LicenseVerificationToken Verify(string licenseKey, int productId, string hostname)
        {
            var sendOwlApiEndpoint = ConfigurationManager.AppSettings["SendOwlApiEndpoint"];
            var sendOwlApiKey = ConfigurationManager.AppSettings["SendOwlApiKey"];
            var sendOwlApiSecret = ConfigurationManager.AppSettings["SendOwlApiSecret"];
            var tokenSigningCertificateThumbprint = ConfigurationManager.AppSettings["TokenSigningCertificateThumbprint"];

            var service = new LicenseService(sendOwlApiEndpoint, sendOwlApiKey, sendOwlApiSecret, tokenSigningCertificateThumbprint);

            try
            {
                return service.VerifyLicense(licenseKey, productId, hostname);
            }
            catch (LicenseVerificationException ex)
            {
                switch (ex.Error)
                {
                    case LicenseVerificationError.UnknownLicenseKey:
                        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound) { ReasonPhrase = ex.Message });

                    case LicenseVerificationError.HostnameMismatch:
                        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden) { ReasonPhrase = ex.Message });

                    default:
                        throw;
                }
            }
        }

        [HttpGet]
        [Route("test")]
        public HttpResponseMessage Test()
        {
            var testProductId = ConfigurationManager.AppSettings["TestProductId"];
            var testHostname = ConfigurationManager.AppSettings["TestHostname"];
            var testKey = ConfigurationManager.AppSettings["TestKey"];

            try
            {
                var token = Verify(testKey, Int32.Parse(testProductId), testHostname);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError) { ReasonPhrase = $"Verification for a supposedly valid license failed: {ex.Message}." };
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
