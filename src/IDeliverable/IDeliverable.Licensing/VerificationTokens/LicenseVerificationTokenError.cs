namespace IDeliverable.Licensing.VerificationTokens
{
    public enum LicenseVerificationTokenError
    {
        UnknownLicenseKey,
        HostnameMismatch,
        NoActiveSubscription,
        LicenseRevoked,
        LicenseServiceError,
        LicenseServiceUnreachable,
        UnexpectedError
    }
}