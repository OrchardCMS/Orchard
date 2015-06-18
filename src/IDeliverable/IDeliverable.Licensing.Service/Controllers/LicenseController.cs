using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using IDeliverable.Licensing.Exceptions;
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
                        ThrowHttpResponseException(ex, HttpStatusCode.NotFound);
                        break;

                    case LicenseVerificationError.HostnameMismatch:
                        ThrowHttpResponseException(ex, HttpStatusCode.Forbidden);
                        break;

                    case LicenseVerificationError.NoActiveSubscription:
                    case LicenseVerificationError.LicenseRevoked:
                        ThrowHttpResponseException(ex, HttpStatusCode.Gone);
                        break;

                    default:
                        throw;
                }
            }
            return null;
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

        private static LicenseService GetLicenseService()
        {
            var sendOwlApiEndpoint = ConfigurationManager.AppSettings["SendOwlApiEndpoint"];
            var sendOwlApiKey = ConfigurationManager.AppSettings["SendOwlApiKey"];
            var sendOwlApiSecret = ConfigurationManager.AppSettings["SendOwlApiSecret"];
            var tokenSigningCertificateThumbprint = ConfigurationManager.AppSettings["TokenSigningCertificateThumbprint"];

            return new LicenseService(sendOwlApiEndpoint, sendOwlApiKey, sendOwlApiSecret, tokenSigningCertificateThumbprint);
        }

        private static void ThrowHttpResponseException(LicenseVerificationException ex, HttpStatusCode httpStatusCode)
        {
            var response = new HttpResponseMessage(httpStatusCode) { ReasonPhrase = ex.Message };
            response.Headers.Add("LicensingErrorCode", ex.Error.ToString());
            throw new HttpResponseException(response);
        }
    }
}
