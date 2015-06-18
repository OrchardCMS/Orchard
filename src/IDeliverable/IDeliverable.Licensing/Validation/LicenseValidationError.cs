namespace IDeliverable.Licensing.Validation
{
    public enum LicenseValidationError
    {
        UnknownLicenseKey,
        HostnameMismatch,
        NoActiveSubscription,
        LicenseRevoked,
        LicensingServiceError,
        LicensingServiceUnreachable,
        TokenAgeValidationFailed,
        TokenSignatureValidationFailed,
        UnexpectedError,
    }
}