using System;

namespace IDeliverable.LicensingService.Exceptions
{
    public class LicenseValidationException : Exception
    {
        public LicenseValidationError Error { get; private set; }

        public LicenseValidationException(string message, LicenseValidationError error) : base(message)
        {
            Error = error;
        }
    }
}