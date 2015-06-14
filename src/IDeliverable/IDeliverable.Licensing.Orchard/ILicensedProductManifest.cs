using Orchard;

namespace IDeliverable.Licensing.Orchard
{
    public interface ILicensedProductManifest : IDependency
    {
        string ProductId { get; }
        string ProductName { get; }
        bool SkipValidationForLocalRequests { get; }
        string LicenseKey { get; }
    }
}
