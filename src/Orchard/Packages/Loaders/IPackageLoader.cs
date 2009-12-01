namespace Orchard.Packages.Loaders {
    public interface IPackageLoader {
        int Order { get; }
        PackageEntry Load(PackageDescriptor descriptor);
    }
}