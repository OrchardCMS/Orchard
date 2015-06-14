using IDeliverable.Licensing.VerificationTokens;
using Orchard.FileSystems.AppData;

namespace IDeliverable.Licensing.Orchard
{
    public class LicenseVerificationTokenStore : ILicenseVerificationTokenStore
    {
        public LicenseVerificationTokenStore(IAppDataFolder appDataFolder)
        {
            mAppDataFolder = appDataFolder;
        }

        private readonly IAppDataFolder mAppDataFolder;

        public LicenseVerificationToken Load(string productId)
        {
            var path = GetRelativeTokenFilePath(productId);
            if (!mAppDataFolder.FileExists(path))
                return null;

            var tokenBase64 = mAppDataFolder.ReadFile(path);
            var token = LicenseVerificationToken.FromBase64(tokenBase64);

            return token;
        }

        public void Save(string productId, LicenseVerificationToken token)
        {
            Clear(productId);

            var path = GetRelativeTokenFilePath(productId);

            var tokenBase64 = token.ToBase64();
            mAppDataFolder.CreateFile(path, tokenBase64);
        }

        public void Clear(string productId)
        {
            var path = GetRelativeTokenFilePath(productId);
            if (mAppDataFolder.FileExists(path))
                mAppDataFolder.DeleteFile(path);
        }

        private string GetRelativeTokenFilePath(string productId)
        {
            return $"IDeliverable.Licensing/{productId}.lic";
        }
    }
}