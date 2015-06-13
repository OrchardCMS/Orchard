using IDeliverable.Licensing.VerificationTokens;
using Orchard.FileSystems.AppData;

namespace IDeliverable.Licensing.Orchard
{
    // TODO: Add caching and cache invalidation to this class.
    //context.ValidFor = TimeSpan.FromMinutes(5);
    //context.CacheDependency = new CacheDependency(_licenseFileManager.GetPhysicalPath(product.ExtensionName));

    public class LicenseVerificationTokenStore : ILicenseVerificationTokenStore
    {
        public LicenseVerificationTokenStore(IAppDataFolder appDataFolder)
        {
            _appDataFolder = appDataFolder;
        }

        private readonly IAppDataFolder _appDataFolder;

        public LicenseVerificationToken Load(string productId)
        {
            var path = GetRelativeTokenFilePath(productId);
            if (!_appDataFolder.FileExists(path))
                return null;

            var tokenBase64 = _appDataFolder.ReadFile(path);
            var token = LicenseVerificationToken.FromBase64(tokenBase64);

            return token;
        }

        public void Save(string productId, LicenseVerificationToken token)
        {
            Clear(productId);

            var path = GetRelativeTokenFilePath(productId);

            var tokenBase64 = token.ToBase64();
            _appDataFolder.CreateFile(path, tokenBase64);
        }

        public void Clear(string productId)
        {
            var path = GetRelativeTokenFilePath(productId);
            if (_appDataFolder.FileExists(path))
                _appDataFolder.DeleteFile(path);
        }

        private string GetRelativeTokenFilePath(string productid)
        {
            return $"IDeliverable.Licensing/{productid}.lic";
        }
    }
}