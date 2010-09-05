using System.Collections.Generic;
using System.Linq;

namespace Orchard.DisplayManagement.Descriptors {
    public class ShapeTableBuilder {
        readonly IList<ShapeDescriptorAlterationBuilderImpl> _descriptorBuilders = new List<ShapeDescriptorAlterationBuilderImpl>();

        public ShapeDescriptorAlterationBuilder Describe {
            get {
                var db = new ShapeDescriptorAlterationBuilderImpl();
                _descriptorBuilders.Add(db);
                return db;
            }
        }

        public IEnumerable<ShapeDescriptorAlteration> Build() {
            return _descriptorBuilders.Select(b => b.Build());
        }

        class ShapeDescriptorAlterationBuilderImpl : ShapeDescriptorAlterationBuilder {
            public ShapeDescriptorAlteration Build() {
                return new ShapeDescriptorAlteration(_shapeType, _feature, _configurations.ToArray());
            }
        }
    }
}