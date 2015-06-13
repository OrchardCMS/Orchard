using System;

namespace IDeliverable.Licensing.Validation
{
    public class LicenseValidationException : Exception
    {
        public LicenseValidationException(LicenseValidationError error)
            :this("An error occurred while validating the license.", error)
        {
        }

        public LicenseValidationException(string message, LicenseValidationError error) : base(message)
        {
            Error = error;
        }

        public LicenseValidationException(LicenseValidationError error, Exception innerException)
            : this("An error occurred while validating the license.", error, innerException)
        {
        }

        public LicenseValidationException(string message, LicenseValidationError error, Exception innerException) : base(message, innerException)
        {
            Error = error;
        }

        public LicenseValidationError Error { get; private set; }
    }
}