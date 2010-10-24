namespace Orchard.UI.Widgets {
    public interface IRuleManager : IDependency {
        bool Matches(string expression);
    }
}
