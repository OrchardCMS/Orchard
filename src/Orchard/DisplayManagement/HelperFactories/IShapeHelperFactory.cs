namespace Orchard.DisplayManagement {
    public interface IShapeHelperFactory : IDependency {
        ShapeHelper CreateShapeHelper();
    }
}