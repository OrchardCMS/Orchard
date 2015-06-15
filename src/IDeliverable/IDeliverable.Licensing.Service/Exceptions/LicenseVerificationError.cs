namespace IDeliverable.Licensing.Service.Exceptions
{
    internal enum LicenseVerificationError
    {
        UnknownLicenseKey,
        NoActiveSubscription,
        HostnameMismatch,
        UnhandledException
    }
}