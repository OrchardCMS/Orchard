using System;

namespace IDeliverable.Licensing.Exceptions
{
    public class LicenseVerificationException : Exception
    {
        public LicenseVerificationException(LicenseVerificationError error)
            :this("An error occurred while verifying the license.", error)
        {
        }

        public LicenseVerificationException(string message, LicenseVerificationError error) : base(message)
        {
            Error = error;
        }

        public LicenseVerificationException(LicenseVerificationError error, Exception innerException)
            : this("An error occurred while verifying the license.", error, innerException)
        {
        }

        public LicenseVerificationException(string message, LicenseVerificationError error, Exception innerException) : base(message, innerException)
        {
            Error = error;
        }

        public LicenseVerificationError Error { get; private set; }
    }
}