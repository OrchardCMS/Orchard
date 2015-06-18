namespace IDeliverable.Licensing.Exceptions
{
    public enum LicenseVerificationError
    {
        UnknownLicenseKey,
        NoActiveSubscription,
        LicenseRevoked,
        HostnameMismatch,
        UnhandledException
    }
}