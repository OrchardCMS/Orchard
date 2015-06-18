using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using IDeliverable.Licensing.Exceptions;

namespace IDeliverable.Licensing.VerificationTokens
{
    public class LicensingServiceClient
    {
        // These are consciously hard coded (need to be obfuscated).
        private static readonly string sLicensingServiceUrl = "https://licensing.ideliverable.com/api/v1/";
        private static readonly string sLicensingServiceApiKey = "MJb17j7YAzSjyEYkhsoI";
        private static readonly string sExpectedServerCertificateThumbprint = "fa4746a778716109e7e80e1b8dc2ed2a2ba3b852";

        public LicenseVerificationToken VerifyLicense(string productId, string licenseKey, string hostname)
        {
            RemoteCertificateValidationCallback certValidationHandler = (sender, certificate, chain, errors) =>
            {
                // Accept the server certificate as long as it matches the expected thumbprint. This makes sure
                // that the API call works even if the client host doesn't trust IDeliverable CA certificate, but
                // fails if somebody is trying to spoof the service.
                var serverCertThumbprint = ((X509Certificate2)certificate).Thumbprint;
                return String.Equals(serverCertThumbprint, sExpectedServerCertificateThumbprint, StringComparison.OrdinalIgnoreCase);
            };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(sLicensingServiceUrl);
                client.DefaultRequestHeaders.Add("ApiKey", sLicensingServiceApiKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback += certValidationHandler;

                try
                {
                    HttpResponseMessage response;

                    try
                    {
                        response = client.GetAsync($"license/{licenseKey}/verify?productId={productId}&hostname={hostname}").Result;
                    }
                    catch (Exception ex)
                    {
                        throw new LicenseVerificationTokenException("An error occurred while calling the licensing service.", LicenseVerificationTokenError.LicenseServiceUnreachable, ex);
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        var licenseVerificationError = ReadLicenseVerificationError(response);
                        
                        switch (licenseVerificationError)
                        {
                            case LicenseVerificationError.UnknownLicenseKey:
                                throw new LicenseVerificationTokenException(response.ReasonPhrase, LicenseVerificationTokenError.UnknownLicenseKey);

                            case LicenseVerificationError.HostnameMismatch:
                                throw new LicenseVerificationTokenException(response.ReasonPhrase, LicenseVerificationTokenError.HostnameMismatch);

                            case LicenseVerificationError.NoActiveSubscription:
                                throw new LicenseVerificationTokenException(response.ReasonPhrase, LicenseVerificationTokenError.NoActiveSubscription);

                            case LicenseVerificationError.LicenseRevoked:
                                throw new LicenseVerificationTokenException(response.ReasonPhrase, LicenseVerificationTokenError.LicenseRevoked);

                            default:
                                throw new LicenseVerificationTokenException(response.ReasonPhrase, LicenseVerificationTokenError.LicenseServiceError);
                        }
                    }

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

        private static LicenseVerificationError? ReadLicenseVerificationError(HttpResponseMessage response)
        {
            IEnumerable<string> values;
            if (!response.Headers.TryGetValues("LicensingErrorCode", out values))
                return null;

            var value = values.First();
            return (LicenseVerificationError)Enum.Parse(typeof(LicenseVerificationError), value);
        }
    }
}