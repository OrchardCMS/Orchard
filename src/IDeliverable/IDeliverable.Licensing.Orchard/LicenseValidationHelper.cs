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
            _products = products;
            _appDataFolder = appDataFolder;
            _tokenStore = new LicenseVerificationTokenStore(_appDataFolder);
            _tokenAccessor = new LicenseVerificationTokenAccessor(_tokenStore);
            _licenseValidator = new LicenseValidator(_tokenAccessor);
        }

        private readonly IEnumerable<ILicensedProductManifest> _products;
        private readonly IAppDataFolder _appDataFolder;
        private readonly LicenseVerificationTokenStore _tokenStore;
        private readonly LicenseVerificationTokenAccessor _tokenAccessor;
        private readonly LicenseValidator _licenseValidator;

        public void ValidateLicense(string productId)
        {
            // TODO: Throttle this code by caching its outcome (void or exception) for 5 minutes.

            var productManifest = _products.Single(x => x.ProductId == productId);

            var options = LicenseValidationOptions.Default;
            if (productManifest.SkipValidationForLocalRequests)
                options = options | LicenseValidationOptions.SkipForLocalRequests;

            _licenseValidator.ValidateLicense(productManifest.ProductId, productManifest.LicenseKey, options);
        }
    }
}
