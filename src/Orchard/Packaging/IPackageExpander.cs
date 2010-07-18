using System.IO;

namespace Orchard.Packaging {
    public interface IPackageExpander : IDependency {
        void ExpandPackage(Stream packageStream);
    }
}