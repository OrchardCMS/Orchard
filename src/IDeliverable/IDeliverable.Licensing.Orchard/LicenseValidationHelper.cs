using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IDeliverable.Licensing.Validation;
using IDeliverable.Licensing.VerificationTokens;
using Orchard;
using Orchard.FileSystems.AppData;

namespace IDeliverable.Licensing.Orchard
{
    public class LicenseValidationHelper
    {
        public static bool GetLicenseIsValid(string productId)
        {
            try
            {
                EnsureLicenseIsValid(productId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void EnsureLicenseIsValid(string productId)
        {
            Instance.ValidateLicense(productId);
        }

        private static LicenseValidationHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    var workContext = HttpContext.Current.Request.RequestContext.GetWorkContext();
                    var products = workContext.Resolve<IEnumerable<ILicensedProductManifest>>();
                    var appDataFolder = workContext.Resolve<IAppDataFolder>();
                    _instance = new LicenseValidationHelper(products, appDataFolder);
                }

                return _instance;
            }
        }

        private static LicenseValidationHelper _instance;

        public LicenseValidationHelper(IEnumerable<ILicensedProductManifest> products, IAppDataFolder appDataFolder)
        {
            var tokenStore = new LicenseVerificationTokenStore(appDataFolder);
            var tokenAccessor = new LicenseVerificationTokenAccessor(tokenStore);

            _products = products;
            _licenseValidator = new LicenseValidator(tokenAccessor);
            _cacheService = new CacheService();
        }

        private readonly IEnumerable<ILicensedProductManifest> _products;
        private readonly LicenseValidator _licenseValidator;
        private readonly CacheService _cacheService;

        public void ValidateLicense(string productId)
        {
            var productManifest = _products.Single(x => x.ProductId == productId);

            string cacheKey = $"ValidateLicenseResult-{productId}-{productManifest.LicenseKey}-{productManifest.SkipValidationForLocalRequests}";
            _cacheService.GetValue(cacheKey, context =>
            {
                context.ValidFor = TimeSpan.FromMinutes(5);

                var options = LicenseValidationOptions.Default;
                if (productManifest.SkipValidationForLocalRequests)
                    options = options | LicenseValidationOptions.SkipForLocalRequests;

                _licenseValidator.ValidateLicense(productManifest.ProductId, productManifest.LicenseKey, options);

                return true;
            });
        }
    }
}
