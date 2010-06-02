namespace Orchard.Localization.Services {
    public interface IResourceManager : IDependency {
        string GetLocalizedString(string key, string cultureName);
    }
}
