namespace Orchard.Localization {
    public interface IResourceManager : IDependency {
        string GetLocalizedString(string key, string cultureName);
    }
}
