namespace Orchard.Conditions.Services {
    public interface IConditionManager : IDependency {
        bool Matches(string expression);
    }
}