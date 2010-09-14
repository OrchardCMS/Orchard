namespace Orchard.DisplayManagement.Descriptors {

    public interface IShapeTableManager : IDependency {
        ShapeTable GetShapeTable(string themeName);
    }

    public interface IShapeTableProvider : IDependency {
        void Discover(ShapeTableBuilder builder);
    }
}
