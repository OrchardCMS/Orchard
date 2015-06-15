using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IDeliverable.Licensing.Validation;
using IDeliverable.Licensing.VerificationTokens;
using Orchard;
using Orchard.Logging;
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

        public static LicenseValidationHelper Instance
        {
            get
            {
                var workContext = HttpContext.Current.Request.RequestContext.GetWorkContext();
                var products = workContext.Resolve<IEnumerable<ILicensedProductManifest>>();
                var appDataFolder = workContext.Resolve<IAppDataFolder>();
                var instance = new LicenseValidationHelper(products, appDataFolder);

                return instance;
            }
        }

        private static readonly TimeSpan _validationResultCachedFor = TimeSpan.FromMinutes(5);

        public LicenseValidationHelper(IEnumerable<ILicensedProductManifest> products, IAppDataFolder appDataFolder)
        {
            var tokenStore = new LicenseVerificationTokenStore(appDataFolder);
            var tokenAccessor = new LicenseVerificationTokenAccessor(tokenStore);

            mProducts = products;
            mLicenseValidator = new LicenseValidator(tokenAccessor);
            mCacheService = new CacheService();
        }

        private readonly IEnumerable<ILicensedProductManifest> mProducts;
        private readonly LicenseValidator mLicenseValidator;
        private readonly CacheService mCacheService;

        public void ValidateLicense(string productId)
        {
            var productManifest = GetLicensedProductManifest(productId);
            productManifest.Logger.Debug("Validating license for product '{0}'...", productId);

            try
            {
                var cacheKey = ComputeCacheKey(productManifest);
                mCacheService.GetValue(cacheKey, context =>
                {
                    context.ValidFor = _validationResultCachedFor;

                    var options = LicenseValidationOptions.Default;
                    if (productManifest.SkipValidationForLocalRequests)
                        options = options | LicenseValidationOptions.SkipForLocalRequests;

                    productManifest.Logger.Debug("Validation result not in cache. Invoking the license validator for product '{0}'...", productId);
                    mLicenseValidator.ValidateLicense(productManifest.ProductId, productManifest.LicenseKey, options);

                    return true;
                });

                productManifest.Logger.Debug("License successfully validated for product '{0}'.", productId);
            }
            catch (Exception ex)
            {
                productManifest.Logger.Error(ex, "An error occurred while validating the license for product '{0}'.", productId);
                throw;
            }
        }

        public void ClearLicenseValidationResult(string productId)
        {
            var productManifest = GetLicensedProductManifest(productId);
            productManifest.Logger.Debug("Clearing license validation result for product '{0}'...", productId);

            var cacheKey = ComputeCacheKey(productManifest);
            mCacheService.RemoveValue(cacheKey);
        }

        private ILicensedProductManifest GetLicensedProductManifest(string productId)
        {
            return mProducts.Single(x => x.ProductId == productId);
        }

        private string ComputeCacheKey(ILicensedProductManifest productManifest)
        {
            return $"ValidateLicenseResult-{productManifest.ProductId}-{productManifest.LicenseKey}-{productManifest.SkipValidationForLocalRequests}";
        }
    }
}
