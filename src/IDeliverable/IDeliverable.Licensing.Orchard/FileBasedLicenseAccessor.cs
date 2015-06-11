using System;
using Orchard.Caching;

namespace IDeliverable.Licensing.Orchard
{
    public class FileBasedLicenseAccessor : ILicenseAccessor
    {
        private readonly ILicenseFileService _licenseFileService;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly IProductManifestManager _productManifestManager;

        public FileBasedLicenseAccessor(
            ILicenseFileService licenseFileService,
            ICacheManager cacheManager,
            ISignals signals,
            IProductManifestManager productManifestManager)
        {
            _licenseFileService = licenseFileService;
            _cacheManager = cacheManager;
            _signals = signals;
            _productManifestManager = productManifestManager;
        }

        private string ComputeLicenseCacheKey(ProductManifest productManifest) => $"License-{productManifest.ExtensionName}";

        public ILicense GetLicense(ProductManifest productManifest)
        {
            return new License
            {
                ProductId = productManifest.ProductId,
                Key = GetLicenseFile(productManifest).Key
            };
        }

        public void UpdateLicense(ProductManifest productManifest, string key)
        {
            var file = GetLicenseFile(productManifest);
            file.Key = key;
            _licenseFileService.Save(file);
            _signals.Trigger(ComputeLicenseCacheKey(productManifest));
        }

        public LicenseValidationToken GetLicenseValidationToken(ProductManifest productManifest)
        {
            var licenseFile = GetLicenseFile(productManifest);

            if (String.IsNullOrWhiteSpace(licenseFile.LicenseValidationToken))
                return null;

            var serializedToken = licenseFile.LicenseValidationToken;
            var token = LicenseValidationToken.Deserialize(serializedToken);

            return token;
        }

        public void UpdateLicenseValidationToken(LicenseValidationToken token)
        {
            var productManifest = _productManifestManager.FindByProductId(token.Info.ProductId);
            var file = _licenseFileService.Load(productManifest.ExtensionName);

            file.LicenseValidationToken = token.Serialize();
            _licenseFileService.Save(file);
            _signals.Trigger(ComputeLicenseCacheKey(productManifest));
        }

        public LicenseFile GetLicenseFile(ProductManifest productManifest)
        {
            return _cacheManager.Get(productManifest.ExtensionName, context =>
            {
                context.Monitor(_signals.When(ComputeLicenseCacheKey(productManifest)));
                context.Monitor(_licenseFileService.WhenPathChanges(productManifest.ExtensionName));

                return _licenseFileService.Load(productManifest.ExtensionName);
            });
        }
    }
}