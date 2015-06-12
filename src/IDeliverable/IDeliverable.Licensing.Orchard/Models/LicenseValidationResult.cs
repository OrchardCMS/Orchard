namespace IDeliverable.Licensing.Orchard.Models
{
    public class LicenseValidationResult
    {
        public static LicenseValidationResult Invalid(LicenseValidationError error)
        {
            return new LicenseValidationResult(error);
        }

        public static LicenseValidationResult Valid()
        {
            return new LicenseValidationResult();
        }

        public static LicenseValidationResult LocalHost()
        {
            return new LicenseValidationResult(localHost: true);
        }

        private LicenseValidationResult(LicenseValidationError? error = null, bool localHost = false)
        {
            Error = error;
        }

        public LicenseValidationError? Error { get; }
        public bool IsValid => Error == null;
        public bool IsLocalHost { get; set; }
    }
}