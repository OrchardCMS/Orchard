namespace Orchard.Localization.Services {
    public interface ICultureStorageProvider : IDependency {
        void SetCulture(string culture);
        string GetCulture();
    }
}