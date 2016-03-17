using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Layout;
using Orchard.Projections.Models;
using Orchard.Projections.Services;

namespace Orchard.Projections.Providers.Layouts {
    public class GridLayout : ILayoutProvider {
        private readonly IContentManager _contentManager;
        protected dynamic Shape { get; set; }

        public GridLayout(IShapeFactory shapeFactory, IContentManager contentManager) {
            _contentManager = contentManager;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeLayoutContext describe) {
            describe.For("Html", T("Html"),T("Html Layouts"))
                .Element("Grid", T("Grid"), T("Organizes content items in a grid."),
                    DisplayLayout,
                    RenderLayout,
                    "GridLayout"
                );
        }

        public LocalizedString DisplayLayout(LayoutContext context) {
            string columns = context.State.Columns;
            bool horizontal = Convert.ToString(context.State.Alignment) != "vertical";

            return horizontal
                       ? T("{0} columns grid", columns)
                       : T("{0} lines grid", columns);
        }

        public dynamic RenderLayout(LayoutContext context, IEnumerable<LayoutComponentResult> layoutComponentResults) {
            int columns = Convert.ToInt32(context.State.Columns);
            bool horizontal = Convert.ToString(context.State.Alignment) != "vertical"; 
            
            string gridTag = Convert.ToString(context.State.GridTag);
            string gridClass = Convert.ToString(context.State.GridClass);
            if (!String.IsNullOrEmpty(gridClass)) gridClass += " ";
            gridClass += "projector-layout projector-grid-layout";
            string gridId = Convert.ToString(context.State.GridId);

            string rowTag = Convert.ToString(context.State.RowTag);
            string rowClass = Convert.ToString(context.State.RowClass);

            string cellTag = Convert.ToString(context.State.CellTag);
            string cellClass = Convert.ToString(context.State.CellClass);

            string emptyCell = Convert.ToString(context.State.EmptyCell);

            IEnumerable<dynamic> shapes =
               context.LayoutRecord.Display == (int)LayoutRecord.Displays.Content
                   ? layoutComponentResults.Select(x => _contentManager.BuildDisplay(x.ContentItem, context.LayoutRecord.DisplayType))
                   : layoutComponentResults.Select(x => x.Properties);

            return Shape.Grid(Id: gridId, Horizontal: horizontal, Columns: columns, Items: shapes, Tag: gridTag, Classes: new[] { gridClass }, RowTag: rowTag, RowClasses: new[] { rowClass }, CellTag: cellTag, CellClasses: new[] { cellClass }, EmptyCell: emptyCell);
        }
    }
}