using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.DisplayManagement {
    public interface IShapeTableFactory : ISingletonDependency {
        ShapeTable CreateShapeTable();
    }
}
