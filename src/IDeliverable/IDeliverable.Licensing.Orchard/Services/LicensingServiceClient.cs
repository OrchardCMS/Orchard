using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using IDeliverable.Licensing.Orchard.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Orchard.Logging;

namespace IDeliverable.Licensing.Orchard.Services
{
    public class LicensingServiceClient : ILicensingServiceClient
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LicensingServiceClient(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            Logger = NullLogger.Instance;
            Thumbprint = "8ef6a50968a338fbe24ffce1d72dceb9fe3355bc";
            ApiEndpoint = "https://licensing.ideliverable.com/api/v1/";
        }

        public ILogger Logger { get; set; }
        public string ApiEndpoint { get; set; }
        public string Thumbprint { get; set; }

        public LicenseValidationToken GetToken(ILicense license)
        {
            return GetToken(license.ProductId, license.Key);
        }

        public LicenseValidationToken GetToken(int productId, string key)
        {
            var request = _httpContextAccessor.Current().Request;
            return ValidateLicenseKey(productId, request.GetHttpHost(), key);
        }

        private LicenseValidationToken ValidateLicenseKey(int productId, string hostname, string key)
        {
            RemoteCertificateValidationCallback handler = delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
            {
                var cert = certificate as X509Certificate2;
                var thumbPrint = cert?.Thumbprint;
                return String.Equals(thumbPrint, Thumbprint, StringComparison.OrdinalIgnoreCase);
            };

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ApiEndpoint);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    ServicePointManager.ServerCertificateValidationCallback += handler;
                    var response = client.GetAsync($"validate/{productId}/{hostname}/{key}").Result;
                    response.EnsureSuccessStatusCode();

                    var responseText = response.Content.ReadAsStringAsync().Result;
                    var token = JsonConvert.DeserializeObject<LicenseValidationToken>(responseText, new StringEnumConverter());
                    ServicePointManager.ServerCertificateValidationCallback -= handler;

                    if (token.Error != null)
                        throw new LicenseValidationException("Invalid license.", token.Error.Value);

                    return token;
                }
            }
            catch (Exception ex)
            {
                throw new LicenseValidationException($"An error occurred while validating the following license key: {key}. Please check the inner exception for more information.", ex);
            }
        }
    }
}