using System;

namespace IDeliverable.Licensing
{
    [Flags]
    public enum LicenseValidationOptions
    {
        Default = 1,
        SkipLocalRequests = 1,
        RefreshToken = 2,
    }
}