namespace IDeliverable.Licensing.VerificationTokens
{
    public interface ILicenseVerificationTokenStore
    {
        LicenseVerificationToken Load(string productId);
        void Save(string productId, LicenseVerificationToken token);
        void Clear(string productId);
    }
}