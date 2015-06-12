using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json.Linq;

namespace IDeliverable.Licensing.Service.Services
{
    public class LicenseService : ILicenseService
    {
        public LicenseService(string sendOwlApiEndpoint, string sendOwlApiKey, string sendOwlApiSecret, string tokenSigningCertificateThumbprint)
        {
            SendOwlApiEndpoint = sendOwlApiEndpoint;
            SendOwlApiKey = sendOwlApiKey;
            SendOwlApiSecret = sendOwlApiSecret;
            TokenSigningCertificateThumbprint = tokenSigningCertificateThumbprint;
        }

        public string SendOwlApiEndpoint { get; set; }
        public string SendOwlApiKey { get; set; }
        public string SendOwlApiSecret { get; set; }
        public string TokenSigningCertificateThumbprint { get; set; }

        /// <exception cref="LicenseValidationException"></exception>
        public LicenseValidationToken ValidateLicense(int productId, string hostname, string key)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(SendOwlApiEndpoint);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", CreateBasicAuthenticationToken(SendOwlApiKey, SendOwlApiSecret));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var licenses = ParseLicenseInfo(client.GetAsync($"products/{productId}/licenses/check_valid?key={key}").Result.Content.ReadAsStringAsync().Result).ToList();

                if (!licenses.Any())
                    throw new LicenseValidationException($"No license with key {key} was found.", LicenseValidationError.UnknownLicenseKey);

                var orderId = licenses.First(x => x.Key == key).OrderId;
                var order = ParseOrderInfo(client.GetAsync($"orders/{orderId}").Result.Content.ReadAsStringAsync().Result);

                if (!order.Hostnames.Contains(hostname, StringComparer.OrdinalIgnoreCase))
                    throw new LicenseValidationException($"No license for the hostname {hostname} was found.", LicenseValidationError.HostnameMismatch);

                var token = GenerateToken(productId, hostname, key);
                return token;
            }
        }

        private LicenseValidationToken GenerateToken(int productId, string hostname, string key)
        {
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            var cert = store.Certificates.Find(X509FindType.FindByThumbprint, TokenSigningCertificateThumbprint, validOnly: false)[0];
            var info = new LicenseValidationInfo(productId, hostname, key, DateTime.UtcNow.Ticks);
            var token = LicenseValidationToken.Create(info, cert);
            store.Close();

            return token;
        }

        private static string CreateBasicAuthenticationToken(string userName, string password)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}"), Base64FormattingOptions.None);
        }

        private static IEnumerable<LicenseInfo> ParseLicenseInfo(string json)
        {
            var licenses = JArray.Parse(json);

            return licenses.Select(node => new LicenseInfo
            {
                Key = (string)node["license"]["key"],
                OrderId = (int)node["license"]["order_id"]
            });
        }

        private static OrderInfo ParseOrderInfo(string json)
        {
            var order = JObject.Parse(json)["order"];
            var customFields = (JArray)order["order_custom_checkout_fields"];
            var hostnames = new List<string>();

            if (customFields.Count > 0)
                hostnames.Add((string)customFields[0]["order_custom_checkout_field"]["value"]);

            if (customFields.Count > 1)
                hostnames.Add((string)customFields[1]["order_custom_checkout_field"]["value"]);

            return new OrderInfo
            {
                OrderId = (int)order["id"],
                Hostnames = hostnames
            };
        }

        private class LicenseInfo
        {
            public string Key { get; set; }
            public int OrderId { get; set; }
        }

        private class OrderInfo
        {
            public int OrderId { get; set; }
            public IList<string> Hostnames { get; set; }
        }
    }
}