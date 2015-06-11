using System.IO;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Packaging.Services {
    public interface IPackageBuilder : IDependency {
        Stream BuildPackage(ExtensionDescriptor extensionDescriptor);
    }
}