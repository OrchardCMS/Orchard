using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDeliverable.Licensing.Validation;
using IDeliverable.Licensing.VerificationTokens;
using Orchard.FileSystems.AppData;

namespace IDeliverable.Licensing.Orchard
{
    public abstract class LicenseValidationHelper
    {
        public LicenseValidationHelper(IAppDataFolder appDataFolder)
        {
            _appDataFolder = appDataFolder;
        }

        private readonly IAppDataFolder _appDataFolder;

        protected abstract string ProductId { get; }
        protected abstract string LicenseKey { get; }
        protected virtual bool SkipValidationForLocalRequests => false;

        public void ValidateLicense()
        {
            var licenseVerificationTokenStore = new LicenseVerificationTokenStore(_appDataFolder);
            var licenseVerificationTokenAccessor = new LicenseVerificationTokenAccessor(licenseVerificationTokenStore);
            var licenseValidator = new LicenseValidator(licenseVerificationTokenAccessor);

            var options = LicenseValidationOptions.Default;
            if (SkipValidationForLocalRequests)
                options = options | LicenseValidationOptions.SkipForLocalRequests;

            licenseValidator.ValidateLicense(ProductId, LicenseKey, options);
        }
    }
}
