namespace Orchard.Localization.Providers {
    public interface ICultureProvider : IDependency {
        string GetCulture();
        int Priority { get; }
    }
}