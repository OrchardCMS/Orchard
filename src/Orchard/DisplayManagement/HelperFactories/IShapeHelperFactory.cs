namespace Orchard.DisplayManagement {
    public interface IShapeHelperFactory : ISingletonDependency {
        ShapeHelper CreateShapeHelper();
    }
}