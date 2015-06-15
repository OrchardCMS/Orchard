namespace IDeliverable.Licensing.VerificationTokens
{
    public enum LicenseVerificationTokenError
    {
        UnknownLicenseKey,
        HostnameMismatch,
        NoActiveSubscription,
        LicenseServiceError,
        LicenseServiceUnreachable,
        UnexpectedError
    }
}