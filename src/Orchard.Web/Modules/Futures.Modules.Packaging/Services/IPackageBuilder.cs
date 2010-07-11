using System.IO;
using Orchard;
using Orchard.Environment.Extensions.Models;

namespace Futures.Modules.Packaging.Services {
    public interface IPackageBuilder : IDependency {
        Stream BuildPackage(ExtensionDescriptor extensionDescriptor);
    }
}