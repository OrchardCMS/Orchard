using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using IDeliverable.Licensing.Service.Exceptions;
using IDeliverable.Licensing.VerificationTokens;
using Newtonsoft.Json.Linq;

namespace IDeliverable.Licensing.Service.Services
{
    public class LicenseService
    {
        public LicenseService(string sendOwlApiEndpoint, string sendOwlApiKey, string sendOwlApiSecret, string tokenSigningCertificateThumbprint)
        {
            mSendOwlApiEndpoint = sendOwlApiEndpoint;
            mSendOwlApiKey = sendOwlApiKey;
            mSendOwlApiSecret = sendOwlApiSecret;
            mTokenSigningCertificateThumbprint = tokenSigningCertificateThumbprint;
        }

        private string mSendOwlApiEndpoint;
        private string mSendOwlApiKey;
        private string mSendOwlApiSecret;
        private string mTokenSigningCertificateThumbprint;

        public LicenseVerificationToken VerifyLicense(string licenseKey, int productId, string hostname)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(mSendOwlApiEndpoint);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", CreateBasicAuthenticationToken(mSendOwlApiKey, mSendOwlApiSecret));
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var licenses = ParseLicenseInfo(client.GetAsync($"products/{productId}/licenses/check_valid?key={licenseKey}").Result.Content.ReadAsStringAsync().Result).ToList();

                    if (!licenses.Any())
                        throw new LicenseVerificationException($"No license with key '{licenseKey}' was found.", LicenseVerificationError.UnknownLicenseKey);

                    var orderId = licenses.First(x => x.Key == licenseKey).OrderId;
                    var order = ParseOrderInfo(client.GetAsync($"orders/{orderId}").Result.Content.ReadAsStringAsync().Result);

                    if (!order.Hostnames.Contains(hostname, StringComparer.OrdinalIgnoreCase))
                        throw new LicenseVerificationException($"The license with key '{licenseKey}' is not valid for the provided '{hostname}'. Valid hostnames are '{String.Join(",", order.Hostnames)}", LicenseVerificationError.HostnameMismatch);

                    var token = CreateVerificationToken(productId, hostname, licenseKey);

                    return token;
                }
            }
            catch (Exception ex) when (!(ex is LicenseVerificationException))
            {
                throw new LicenseVerificationException(LicenseVerificationError.UnhandledException, ex);
            }
        }

        private LicenseVerificationToken CreateVerificationToken(int productId, string hostname, string key)
        {
            X509Certificate2 signingCert = null;

            // Try to find the certificate in either the CurrentUser or LocalMachine stores
            // to maximize code portability.
            signingCert = GetSigningCertificateFrom(StoreLocation.CurrentUser) ?? GetSigningCertificateFrom(StoreLocation.LocalMachine);
            if (signingCert == null)
                throw new Exception($"No certificate with thumbprint '{mTokenSigningCertificateThumbprint}' was found in the certificate store.");

            var info = new LicenseVerificationInfo(productId, hostname, key, DateTime.UtcNow.Ticks);
            var token = LicenseVerificationToken.Create(info, signingCert);

            return token;
        }

        private X509Certificate2 GetSigningCertificateFrom(StoreLocation location)
        {
            var store = new X509Store(StoreName.My, location);
            store.Open(OpenFlags.ReadOnly);

            try
            {
                var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, mTokenSigningCertificateThumbprint, validOnly: false);
                if (certificates.Count > 0)
                    return certificates[0];

                return null;
            }
            finally
            {
                store.Close();
            }
        }

        private string CreateBasicAuthenticationToken(string userName, string password)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}"), Base64FormattingOptions.None);
        }

        private IEnumerable<LicenseInfo> ParseLicenseInfo(string json)
        {
            var licensesQuery =
                from node in JArray.Parse(json)
                select new LicenseInfo((string)node["license"]["key"], (int)node["license"]["order_id"]);

            return licensesQuery.ToArray();
        }

        private OrderInfo ParseOrderInfo(string json)
        {
            var order = JObject.Parse(json)["order"];
            var customFields = (JArray)order["order_custom_checkout_fields"];
            var hostnames = new List<string>();

            if (customFields.Count > 0)
                hostnames.Add((string)customFields[0]["order_custom_checkout_field"]["value"]);

            if (customFields.Count > 1)
                hostnames.Add((string)customFields[1]["order_custom_checkout_field"]["value"]);

            return new OrderInfo((int)order["id"], hostnames);
        }
    }
}