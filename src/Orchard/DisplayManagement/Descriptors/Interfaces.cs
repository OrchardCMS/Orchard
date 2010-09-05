using System.Collections.Generic;

namespace Orchard.DisplayManagement.Descriptors {

    public interface IShapeTableManager : IDependency {
        ShapeTable GetShapeTable(string themeName);
    }

    public interface IShapeTableFactory : IDependency {
        IDictionary<string, ShapeTable> CreateShapeTables();
    }

    public interface IShapeDescriptorBindingStrategy : IDependency {
        void Discover(ShapeTableBuilder builder);
    }
}
