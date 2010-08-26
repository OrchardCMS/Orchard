namespace Orchard.DisplayManagement {
    public interface IShapeHelperFactory : IDependency {
        dynamic CreateHelper();
    }
}