namespace IDeliverable.Licensing.Service.Exceptions
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