using Orchard;
using Orchard.Caching;

namespace IDeliverable.Licensing.Orchard
{
    public interface ILicenseFileService : IDependency
    {
        LicenseFile Load(string name);
        void Save(LicenseFile file);
        IVolatileToken WhenPathChanges(string extensionName);
    }
}