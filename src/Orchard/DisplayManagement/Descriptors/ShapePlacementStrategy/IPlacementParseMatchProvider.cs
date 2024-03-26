namespace Orchard.DisplayManagement.Descriptors.ShapePlacementStrategy {
    public interface IPlacementParseMatchProvider : IDependency {
        string Key { get; }
        bool Match(ShapePlacementContext context, string expression);
    }
}
