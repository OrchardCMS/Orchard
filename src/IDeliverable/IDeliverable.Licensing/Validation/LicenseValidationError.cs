namespace IDeliverable.Licensing.Validation
{
    public enum LicenseValidationError
    {
        UnknownLicenseKey,
        HostnameMismatch,
        NoActiveSubscription,
        LicensingServiceError,
        LicensingServiceUnreachable,
        TokenAgeValidationFailed,
        TokenSignatureValidationFailed,
        UnexpectedError,
    }
}