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
        [ApiKeyAuthorization]
        public LicenseVerificationToken Verify(string licenseKey, string productId, string hostname)
        {
            var service = GetLicenseService();

            try
            {
                return service.VerifyLicense(licenseKey, productId, hostname, throwOnError: false);
            }
            catch (LicenseVerificationException ex)
            {
                switch (ex.Error)
                {
                    case LicenseVerificationError.UnknownLicenseKey:
                        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound) { ReasonPhrase = ex.Message });

                    case LicenseVerificationError.HostnameMismatch:
                        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden) { ReasonPhrase = ex.Message });

                    case LicenseVerificationError.NoActiveSubscription:
                        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Gone) { ReasonPhrase = ex.Message });

                    default:
                        throw;
                }
            }
        }

        [HttpGet]
        [Route("test")]
        [ApiKeyAuthorization]
        public HttpResponseMessage Test()
        {
            var testKey = ConfigurationManager.AppSettings["TestKey"];
            var testProductId = ConfigurationManager.AppSettings["TestProductId"];
            var testHostname = ConfigurationManager.AppSettings["TestHostname"];
            var service = GetLicenseService();

            try
            {
                service.VerifyLicense(testKey, testProductId, testHostname, throwOnError: true);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError) { ReasonPhrase = $"Verification for a supposedly valid license failed: {ex.Message}." };
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private LicenseService GetLicenseService()
        {
            var sendOwlApiEndpoint = ConfigurationManager.AppSettings["SendOwlApiEndpoint"];
            var sendOwlApiKey = ConfigurationManager.AppSettings["SendOwlApiKey"];
            var sendOwlApiSecret = ConfigurationManager.AppSettings["SendOwlApiSecret"];
            var tokenSigningCertificateThumbprint = ConfigurationManager.AppSettings["TokenSigningCertificateThumbprint"];

            return new LicenseService(sendOwlApiEndpoint, sendOwlApiKey, sendOwlApiSecret, tokenSigningCertificateThumbprint);
        }
    }
}
