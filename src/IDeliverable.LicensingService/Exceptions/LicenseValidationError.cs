namespace IDeliverable.LicensingService.Exceptions
{
    public enum LicenseValidationError
    {
        UnknownLicenseKey,
        HostnameMismatch,
        LicenseExpired,
        LicenseRevoked,
        SignatureValidationFailed,
        UnhandledException
    }
}