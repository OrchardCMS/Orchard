using IDeliverable.Licensing.Orchard.Models;

namespace IDeliverable.Licensing.Orchard.Services
{
    public interface ILicenseFileManager
    {
        string GetRelativePath(string extensionName);
        string GetPhysicalPath(string extensionName);
        LicenseFile Load(string extensionName);
        void Save(LicenseFile file);
    }
}