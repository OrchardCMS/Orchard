using System;

namespace IDeliverable.Licensing.VerificationTokens
{
    public class LicenseVerificationTokenException : Exception
    {
        public LicenseVerificationTokenException(LicenseVerificationTokenError error)
            :this("An error occurred while getting a license verification token.", error)
        {
        }

        public LicenseVerificationTokenException(string message, LicenseVerificationTokenError error) : base(message)
        {
            Error = error;
        }

        public LicenseVerificationTokenException(LicenseVerificationTokenError error, Exception innerException)
            : this("An error occurred while getting a license verification token.", error, innerException)
        {
        }

        public LicenseVerificationTokenException(string message, LicenseVerificationTokenError error, Exception innerException) : base(message, innerException)
        {
            Error = error;
        }

        public LicenseVerificationTokenError Error { get; private set; }
    }
}