using System.Linq;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Services;

namespace Orchard.Layouts.Drivers {
    public class GridDriver : ElementDriver<Grid> {
        private readonly IElementManager _elementManager;

        public GridDriver(IElementManager elementManager) {
            _elementManager = elementManager;
        }

        protected override void OnDisplaying(Grid element, ElementDisplayContext context) {
            var grid = (Grid)context.Element;
            var describeContext = new DescribeElementsContext {Content = context.Content};

            if (!grid.Elements.Any()) {
                // Add a default row.
                var rowDescriptor = _elementManager.GetElementDescriptorByType<Row>(describeContext);
                var row = _elementManager.ActivateElement<Row>(rowDescriptor, new ActivateElementArgs { Container = grid });

                grid.Elements.Add(row);

                // Add a default column.
                var columnDescriptor = _elementManager.GetElementDescriptorByType<Column>(describeContext);
                var column = _elementManager.ActivateElement<Column>(columnDescriptor, new ActivateElementArgs { Container = row });

                column.ColumnSpan = 12;
                row.Elements.Add(column);
            }
        }
    }
}