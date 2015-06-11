namespace IDeliverable.Licensing
{
    public class LicenseValidationResult
    {
        public LicenseValidationResult(LicenseValidationToken token)
        {
            Token = token;
            Error = token.Error;
        }

        public LicenseValidationResult(LicenseValidationError error)
        {
            Error = error;
        }

        public LicenseValidationToken Token { get; private set; }
        public LicenseValidationError? Error { get; }
        public bool IsValid => Error == null;
    }
}