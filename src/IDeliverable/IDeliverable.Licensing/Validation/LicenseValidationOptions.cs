using System;

namespace IDeliverable.Licensing.Validation
{
    [Flags]
    public enum LicenseValidationOptions
    {
        Default = 0,
        SkipForLocalRequests = 1,
        ForceRenewToken = 2,
    }
}