using System;

namespace IDeliverable.Licensing
{
    public class LicenseValidationException : Exception
    {
        public LicenseValidationError Error { get; private set; }

        public LicenseValidationException(string message, LicenseValidationError error) : base(message)
        {
            Error = error;
        }

        public LicenseValidationException(string message, Exception innerException) : base(message, innerException)
        {
            Error = LicenseValidationError.UnhandledException;
        }
    }
}