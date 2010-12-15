using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.DisplayManagement.Descriptors {
    public class ShapeTableBuilder {
        readonly IList<ShapeAlterationBuilder> _alterationBuilders;
        readonly Feature _feature;

        public ShapeTableBuilder(IList<ShapeAlterationBuilder> alterationBuilders, Feature feature) {
            _alterationBuilders = alterationBuilders;
            _feature = feature;
        }

        public ShapeAlterationBuilder Describe(string shapeType) {
            var alterationBuilder = new ShapeAlterationBuilder(_feature, shapeType);
            _alterationBuilders.Add(alterationBuilder);
            return alterationBuilder;
        }

    }
}