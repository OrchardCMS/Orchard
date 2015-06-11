namespace IDeliverable.Licensing.Orchard
{
    public class LicenseViewModel
    {
        public string Hostname { get; set; }
        public string Key { get; set; }
        public bool IsValid { get; set; }
        public LicenseValidationResult LicenseValidationResult { get; set; }
    }
}