namespace IDeliverable.Licensing.Orchard.Services
{
    public interface ILicenseAccessor
    {
        LicenseValidationToken GetLicenseValidationToken(ILicense license, bool refresh);
    }
}