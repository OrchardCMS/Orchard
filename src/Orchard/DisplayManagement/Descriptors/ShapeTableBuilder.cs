using System.Collections.Generic;
using System.Linq;

namespace Orchard.DisplayManagement.Descriptors {
    public class ShapeTableBuilder {
        readonly IList<ShapeAlterationBuilderImpl> _descriptorBuilders = new List<ShapeAlterationBuilderImpl>();

        public ShapeAlterationBuilder Describe {
            get {
                var db = new ShapeAlterationBuilderImpl();
                _descriptorBuilders.Add(db);
                return db;
            }
        }

        public IEnumerable<ShapeAlteration> Build() {
            return _descriptorBuilders.Select(b => b.Build());
        }

        class ShapeAlterationBuilderImpl : ShapeAlterationBuilder {
            public ShapeAlteration Build() {
                return new ShapeAlteration(_shapeType, _feature, _configurations.ToArray());
            }
        }
    }
}