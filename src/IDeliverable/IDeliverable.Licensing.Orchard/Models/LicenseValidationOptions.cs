using System;

namespace IDeliverable.Licensing.Orchard.Models
{
    [Flags]
    public enum LicenseValidationOptions
    {
        Default = 1,
        SkipLocalRequests = 1,
        RefreshToken = 2,
    }
}