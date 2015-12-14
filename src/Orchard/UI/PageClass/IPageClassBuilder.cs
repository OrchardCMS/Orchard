namespace Orchard.UI.PageClass {
    public interface IPageClassBuilder : IDependency {
        void AddClassNames(params object[] classNames);
    }
}