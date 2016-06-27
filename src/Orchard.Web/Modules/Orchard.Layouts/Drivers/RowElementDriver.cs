using System.Collections.Generic;
using System.Linq;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;

namespace Orchard.Layouts.Drivers {
    public class RowElementDriver : ElementDriver<Row> {
        protected override void OnDisplaying(Row element, ElementDisplayingContext context) {
            context.ElementShape.Collapsed = false;
        }

        protected override void OnDisplayed(Row element, ElementDisplayedContext context) {
            var columnShapes = ((IEnumerable<dynamic>)context.ElementShape.Items).ToList();
            var columnIndex = 0;

            foreach (var columnShape in columnShapes) {
                var column = (Column)columnShape.Element;

                if (column.Collapsible == true && IsEmpty(columnShape)) {
                    columnShape.Collapsed = true;

                    // Get the first non-collapsed sibling column so we can increase its width with the width of the current column being collapsed.
                    var sibling = GetNonCollapsedSibling(columnShapes, columnIndex);
                    if (sibling != null) {
                        // Increase the width of the sibling by the width of the current column.
                        sibling.Width += columnShape.Width;
                    }
                    else {
                        // The row has only one column, which is collapsed, so we hide the row entirely.
                        context.ElementShape.Collapsed = true;
                    } 
                }

                ++columnIndex;
            }
        }

        private dynamic GetNonCollapsedSibling(IList<dynamic> columnShapes, int index) {
            var siblings = index == 0 ? columnShapes : columnShapes.Reverse();
            return siblings.FirstOrDefault(x => x.Collapsed == false);
        }

        private static bool IsEmpty(dynamic shape) {
            return shape.Items.Count == 0;
        }
    }
}