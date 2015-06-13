using System;

namespace IDeliverable.Licensing.VerificationTokens
{
    public class LicenseVerificationTokenAccessor
    {
        private static readonly TimeSpan _tokenRenewalInterval = TimeSpan.FromDays(14);

        public LicenseVerificationTokenAccessor(ILicenseVerificationTokenStore store)
        {
            _store = store;
            _licensingServiceClient = new LicensingServiceClient();
        }

        private readonly ILicenseVerificationTokenStore _store;
        private readonly LicensingServiceClient _licensingServiceClient;

        public LicenseVerificationToken GetLicenseVerificationToken(string productId, string licenseKey, string hostname, bool forceRenew = false)
        {
            if (forceRenew)
                _store.Clear(productId);

            var token = _store.Load(productId);

            // Renew verification token from licensing server if:
            // - We don't have a token in store OR
            // - We have a token in store but it has passed the token renewal interval

            if (token == null || token.Age > _tokenRenewalInterval)
            {
                try
                {
                    token = _licensingServiceClient.VerifyLicense(productId, licenseKey, hostname);
                }
                catch (Exception ex)
                {
                    if (ex is LicenseVerificationTokenException)
                        throw;

                    throw new LicenseVerificationTokenException(LicenseVerificationTokenError.UnexpectedError, ex);
                }

                _store.Save(productId, token);
            }

            return token;
        }
    }
}