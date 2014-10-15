using System.ComponentModel.Design.Serialization;
using System.Linq;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;

namespace Orchard.Layouts.Drivers {
    public class RowDriver : ElementDriver<Row> {
        private readonly IElementManager _elementManager;

        public RowDriver(IElementManager elementManager) {
            _elementManager = elementManager;
        }

        protected override void OnDisplaying(Row element, ElementDisplayContext context) {
            EnsureDefaultColumn(element, context);
            EnsureSpanValues(element);
            HandleColumnResizeEvent(element, context);
        }

        private void EnsureDefaultColumn(Row element, ElementDisplayContext context) {
            if (!element.Columns.Any()) {
                // Add a default column.
                var column = _elementManager.ActivateElement<Column>();
                column.ColumnSpan = Grid.GridSize;
                element.Elements.Add(column);
            }
        }

        private static void EnsureSpanValues(Row element) {
            // Process each column, setting a span value if none is set.
            foreach (var column in element.Columns) {
                var span = column.ColumnSpan;

                if (span == null) {
                    // Get the last column.
                    var lastColumn = element.Columns.LastOrDefault(x => x != column);
                    var lastColumnSpan = lastColumn != null ? lastColumn.ColumnSpan ?? Grid.GridSize : Grid.GridSize;

                    if (lastColumn != null) {
                        lastColumn.ColumnSpan = lastColumnSpan / 2;
                        column.ColumnSpan = lastColumnSpan / 2;
                    }
                }
            }
        }

        private void HandleColumnResizeEvent(Row element, ElementDisplayContext context) {
            if (context.RenderEventName != "span-changed")
                return;

            var columnIndex = context.RenderEventArgs.ToInt32().GetValueOrDefault();
            var column = element.Columns.ElementAtOrDefault(columnIndex);

            if (column == null)
                return;

            var siblingIndex = columnIndex + 1;
            var sibling = element.Columns.ElementAtOrDefault(siblingIndex);

            if (sibling == null)
                return;

            var totalSpanSize = element.CurrentSpanSize;

            if (totalSpanSize > Grid.GridSize) {
                // Decrease the sibling's span.
                var overflow = totalSpanSize - Grid.GridSize;
                var allowedSiblingShrink = sibling.ColumnSpan - overflow >= 1 ? overflow : sibling.ColumnSpan > 1 ? sibling.ColumnSpan - 1 : 0;
                var selfShrink = sibling.ColumnSpan - overflow <= 0 ? overflow - sibling.ColumnSpan : 0;

                sibling.ColumnSpan -= allowedSiblingShrink;
                column.ColumnSpan -= selfShrink;
            }
            else {
                // Increase the sibling's span
                var space = Grid.GridSize - totalSpanSize;
                sibling.ColumnSpan += space;
            }
        }
    }
}