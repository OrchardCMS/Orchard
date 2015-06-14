using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace IDeliverable.Licensing.VerificationTokens
{
    public class LicensingServiceClient
    {
        // These are consciously hard coded.
        private static readonly string LicensingServiceUrl = "https://licensing.ideliverable.com/api/v1/";
        private static readonly string ExpectedServerCertificateThumbprint = "fa4746a778716109e7e80e1b8dc2ed2a2ba3b852";

        public LicenseVerificationToken VerifyLicense(string productId, string licenseKey, string hostname)
        {
            RemoteCertificateValidationCallback certValidationHandler = (sender, certificate, chain, errors) =>
            {
                // Accept the server certificate as long as it matches the expected thumbprint. This makes sure
                // that the API call works even if the client host doesn't trust IDeliverable CA certificate, but
                // fails if somebody is trying to spoof the service.
                var serverCertThumbprint = ((X509Certificate2)certificate).Thumbprint;
                return String.Equals(serverCertThumbprint, ExpectedServerCertificateThumbprint, StringComparison.OrdinalIgnoreCase);
            };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(LicensingServiceUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback += certValidationHandler;

                try
                {
                    HttpResponseMessage response = null;

                    try
                    {
                        response = client.GetAsync($"license/{licenseKey}/verify?productId={productId}&hostname={hostname}").Result;
                    }
                    catch (Exception ex)
                    {
                        throw new LicenseVerificationTokenException("An error occurred while calling the licensing service.", LicenseVerificationTokenError.LicenseServiceUnreachable, ex);
                    }

                    if (response.StatusCode == HttpStatusCode.NotFound)
                        throw new LicenseVerificationTokenException(response.ReasonPhrase, LicenseVerificationTokenError.UnknownLicenseKey);

                    if (response.StatusCode == HttpStatusCode.Forbidden)
                        throw new LicenseVerificationTokenException(response.ReasonPhrase, LicenseVerificationTokenError.HostnameMismatch);

                    if (!response.IsSuccessStatusCode)
                        throw new LicenseVerificationTokenException(response.ReasonPhrase, LicenseVerificationTokenError.LicenseServiceError);

                    var responseText = response.Content.ReadAsStringAsync().Result;
                    var token = LicenseVerificationToken.Parse(responseText);

                    return token;
                }
                finally
                {
                    ServicePointManager.ServerCertificateValidationCallback -= certValidationHandler;   
                }
            }
        }
    }
}