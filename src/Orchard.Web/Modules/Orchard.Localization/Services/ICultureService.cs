namespace Orchard.Localization.Services {
    public interface ICultureService : IDependency {
        void SetCulture(string culture);
        string GetCulture();
    }
}