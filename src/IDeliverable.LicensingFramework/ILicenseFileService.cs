using Orchard;
using Orchard.Caching;

namespace IDeliverable.Licensing
{
    public interface ILicenseFileService : IDependency
    {
        LicenseFile Load(string name);
        void Save(LicenseFile file);
        IVolatileToken WhenPathChanges(string extensionName);
    }
}