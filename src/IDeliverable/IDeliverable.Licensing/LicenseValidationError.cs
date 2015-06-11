namespace IDeliverable.Licensing
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