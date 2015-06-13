namespace IDeliverable.Licensing.VerificationTokens
{
    public enum LicenseVerificationTokenError
    {
        UnknownLicenseKey,
        HostnameMismatch,
        LicenseServiceError,
        LicenseServiceUnreachable,
        UnexpectedError
    }
}