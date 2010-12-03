namespace Orchard.Localization {
    public interface IText : ISingletonDependency {
        LocalizedString Get(string textHint, params object[] args);
    }
}