using System.Collections.Generic;
using System.Linq;

namespace Orchard.DisplayManagement.Descriptors {
    public class DefaultShapeTableManager : IShapeTableManager {
        private readonly IEnumerable<IShapeDescriptorBindingStrategy> _bindingStrategies;

        public DefaultShapeTableManager(IEnumerable<IShapeDescriptorBindingStrategy> bindingStrategies) {
            _bindingStrategies = bindingStrategies;
        }

        private ShapeTable _shapeTable;

        public ShapeTable GetShapeTable(string themeName) {
            if (_shapeTable == null) {
                var builder = new ShapeTableBuilder();
                foreach (var bindingStrategy in _bindingStrategies) {
                    bindingStrategy.Discover(builder);
                }
                // placeholder - alterations will need to be selective and in a particular order 
                
                // GroupBy has been determined to preserve the order of items in original series
                _shapeTable = new ShapeTable {
                    Descriptors = builder.Build()
                        .GroupBy(alteration => alteration.ShapeType)
                        .Select(group => group.Aggregate(
                            new ShapeDescriptor { ShapeType = group.Key },
                            (d, a) => {
                                a.Alter(d);
                                return d;
                            }))
                        .ToDictionary(sd => sd.ShapeType)
                };
            }
            return _shapeTable;
        }
    }
}
