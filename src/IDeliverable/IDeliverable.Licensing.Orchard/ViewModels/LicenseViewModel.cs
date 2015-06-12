using IDeliverable.Licensing.Orchard.Models;

namespace IDeliverable.Licensing.Orchard.ViewModels
{
    public class LicenseViewModel
    {
        public string Hostname { get; set; }
        public string Key { get; set; }
        public bool IsValid { get; set; }
        public LicenseValidationResult LicenseValidationResult { get; set; }
    }
}