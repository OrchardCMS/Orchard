using System;
using System.Web.Caching;

namespace IDeliverable.Licensing.Orchard.Services
{
    public class LicenseAccessor : ILicenseAccessor
    {
        private readonly ILicenseFileManager _licenseFileManager;
        private readonly ILicensingServiceClient _licensingServiceClient;
        private readonly IProductManifestManager _productManifestManager;
        private readonly ICacheService _cacheService;
        private static TimeSpan RenewalInterval => TimeSpan.FromDays(14);

        public LicenseAccessor(
            ILicenseFileManager licenseFileManager,
            IProductManifestManager productManifestManager, ILicensingServiceClient licensingServiceClient, ICacheService cacheService)
        {
            _licenseFileManager = licenseFileManager;
            _productManifestManager = productManifestManager;
            _licensingServiceClient = licensingServiceClient;
            _cacheService = cacheService;
        }

        public LicenseValidationToken GetLicenseValidationToken(ILicense license, bool refresh = false)
        {
            var cacheKey = $"LicenseValidationToken-{license.ProductId}";
            return _cacheService.GetValue(cacheKey, context =>
            {
                var product = _productManifestManager.FindByProductId(license.ProductId);
                var licenseFile = _licenseFileManager.Load(product.ExtensionName);
                var token = licenseFile.GetLicenseValidationToken();

                if (!token.IsEmpty && !refresh)
                {
                    if (TokenIsWithinRenewalInterval(token))
                        return token;
                }

                token = _licensingServiceClient.GetToken(license);
                licenseFile.SetLicenseValidationToken(token);
                _licenseFileManager.Save(licenseFile);

                context.ValidFor = TimeSpan.FromMinutes(5);
                context.CacheDependency = new CacheDependency(_licenseFileManager.GetPhysicalPath(product.ExtensionName));
                return token;
            });
        }

        private static bool TokenIsWithinRenewalInterval(LicenseValidationToken token)
        {
            return token.IssuedSince() <= RenewalInterval;
        }
    }
}