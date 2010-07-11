using System.IO;
using Orchard;

namespace Futures.Modules.Packaging.Services {
    public interface IPackageExpander : IDependency {
        void ExpandPackage(Stream packageStream);
    }
}