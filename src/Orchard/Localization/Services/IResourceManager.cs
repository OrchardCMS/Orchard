namespace Orchard.Localization.Services {
    public interface IResourceManager : IDependency {
        string GetLocalizedString(string scope, string text, string cultureName);
    }
}
