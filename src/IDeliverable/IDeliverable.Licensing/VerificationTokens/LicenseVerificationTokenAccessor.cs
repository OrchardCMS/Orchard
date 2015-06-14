using System;

namespace IDeliverable.Licensing.VerificationTokens
{
    public class LicenseVerificationTokenAccessor
    {
        private static readonly TimeSpan sTokenRenewalInterval = TimeSpan.FromDays(14);

        public LicenseVerificationTokenAccessor(ILicenseVerificationTokenStore store)
        {
            mStore = store;
            mLicensingServiceClient = new LicensingServiceClient();
        }

        private readonly ILicenseVerificationTokenStore mStore;
        private readonly LicensingServiceClient mLicensingServiceClient;

        public LicenseVerificationToken GetLicenseVerificationToken(string productId, string licenseKey, string hostname, bool forceRenew = false)
        {
            var token = mStore.Load(productId);

            // Delete the existing verification token from store if:
            // * It was issued for a different license key OR
            // * We are instructed by caller to force renewal
            if (token != null && (token.Info.LicenseKey != licenseKey || forceRenew))
            {
                mStore.Clear(productId);
                token = null;
            }

            // Try to renew verification token from licensing service if:
            // * We don't have a token in store OR
            // * The one we have has passed the token renewal interval
            if (token == null || token.Age > sTokenRenewalInterval)
            {
                try
                {
                    token = mLicensingServiceClient.VerifyLicense(productId, licenseKey, hostname);
                }
                catch (Exception ex)
                {
                    // If the license key is reported by licensing service to be either invalid or for
                    // a different hostname, we delete any existing token from store and throw unconditionally.
                    if (ex is LicenseVerificationTokenException)
                    {
                        var lvtex = ex as LicenseVerificationTokenException;
                        if (lvtex.Error == LicenseVerificationTokenError.UnknownLicenseKey || lvtex.Error == LicenseVerificationTokenError.HostnameMismatch)
                        {
                            mStore.Clear(productId);
                            throw;
                        }
                    }
                       
                    // If we have an existing token from before, return it rather than throwing.
                    if (token != null)
                        return token;

                    if (ex is LicenseVerificationTokenException)
                        throw;

                    throw new LicenseVerificationTokenException(LicenseVerificationTokenError.UnexpectedError, ex);
                }

                mStore.Save(productId, token);
            }

            return token;
        }
    }
}