namespace IDeliverable.Licensing.Validation
{
    public enum LicenseValidationError
    {
        UnknownLicenseKey,
        HostnameMismatch,
        LicensingServiceError,
        LicensingServiceUnreachable,
        TokenSignatureValidationFailed,
        UnexpectedError
    }
}