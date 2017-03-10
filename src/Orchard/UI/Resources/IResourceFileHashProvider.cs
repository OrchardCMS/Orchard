namespace Orchard.UI.Resources {
    public interface IResourceFileHashProvider : ISingletonDependency {
        string GetResourceFileHash(string physicalPath);
    }
}
