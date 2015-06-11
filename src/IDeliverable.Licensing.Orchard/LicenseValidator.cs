using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Orchard.Caching;
using Orchard.Mvc;
using Orchard.Services;

namespace IDeliverable.Licensing.Orchard
{
    public class LicenseValidator : ILicenseValidator
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILicenseAccessor _licenseAccessor;
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly ISignals _signals;
        private readonly IProductManifestManager _productManifestManager;

        public LicenseValidator(
            IHttpContextAccessor httpContextAccessor, 
            ILicenseAccessor licenseAccessor,
            IProductManifestManager productManifestManager,
            ICacheManager cacheManager,
            IClock clock,
            ISignals signals)
        {
            _httpContextAccessor = httpContextAccessor;
            _licenseAccessor = licenseAccessor;
            _productManifestManager = productManifestManager;
            _cacheManager = cacheManager;
            _clock = clock;
            _signals = signals;

            ApiEndpoint = "https://licensing.ideliverable.com/api/v1/";
            Thumbprint = "8ef6a50968a338fbe24ffce1d72dceb9fe3355bc";
        }

        public string ApiEndpoint { get; set; }
        public string Thumbprint { get; set; }

        public LicenseValidationResult ValidateLicense(ILicense license, LicenseValidationOptions options = LicenseValidationOptions.Default)
        {
            return ValidateLicense(license.ProductId, license.Key, options);
        }

        public LicenseValidationResult ValidateLicense(int productId, string key, LicenseValidationOptions options = LicenseValidationOptions.Default)
        {
            var request = _httpContextAccessor.Current().Request;

            if ((options & LicenseValidationOptions.SkipLocalRequests) == LicenseValidationOptions.SkipLocalRequests)
                if (request.IsLocal)
                    return new LicenseValidationResult(LicenseValidationToken.CreateLocalHostToken(productId));

            var licenseCacheKey = $"License-{productId}";
            var refreshTokenSignal = $"RefreshToken-{productId}";
            var refreshToken = (options & LicenseValidationOptions.RefreshToken) == LicenseValidationOptions.RefreshToken;

            if (refreshToken)
                _signals.Trigger(refreshTokenSignal);

            return _cacheManager.Get(licenseCacheKey, context =>
            {
                context.Monitor(_clock.When(TimeSpan.FromMinutes(5)));
                context.Monitor(_signals.When(refreshTokenSignal));
                try {
                    var token = !refreshToken ? ValidateLicenseToken(productId) : default(LicenseValidationToken);

                    if(token == null)
                    {
                        token = ValidateLicenseKey(productId, request.GetHttpHost(), key);
                        _licenseAccessor.UpdateLicenseValidationToken(token);
                    }

                    return new LicenseValidationResult(token);
                }
                catch(LicenseValidationException ex)
                {
                    return new LicenseValidationResult(ex.Error);
                }
            });
        }

        private LicenseValidationToken ValidateLicenseKey(int productId, string hostname, string key)
        {
            RemoteCertificateValidationCallback handler = delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
            {
                var cert = certificate as X509Certificate2;
                var thumbPrint = cert?.Thumbprint;
                return String.Equals(thumbPrint, Thumbprint, StringComparison.OrdinalIgnoreCase);
            };

            try {
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
            catch(LicenseValidationException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new LicenseValidationException("An exception occurred while trying to contact the licensing service. Please see the inner exception for more information.", ex);
            }
        }

        private LicenseValidationToken ValidateLicenseToken(int productId)
        {
            var cacheKey = $"LicenseValidationToken-{productId}";
            return _cacheManager.Get(cacheKey, context =>
            {
                var productManifest = _productManifestManager.FindByProductId(productId);
                var token = _licenseAccessor.GetLicenseValidationToken(productManifest);

                if (token.GetIsWithinRenewalInterval())
                {
                    context.Monitor(_clock.When(token.RenewalInterval));
                    return token;
                }
                else if (token.GetIsWithinGraceTime())
                {
                    context.Monitor(_clock.When(TimeSpan.FromDays(1)));
                    return token;
                }
                
                return null;
            });
        }
    }
}