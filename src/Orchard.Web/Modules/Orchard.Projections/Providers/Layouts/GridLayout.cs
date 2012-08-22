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
            string rowClass = context.State.RowClass;
            string gridClass = context.State.GridClass;
            string gridId = context.State.GridId;

            IEnumerable<dynamic> shapes =
               context.LayoutRecord.Display == (int)LayoutRecord.Displays.Content
                   ? layoutComponentResults.Select(x => _contentManager.BuildDisplay(x.ContentItem, context.LayoutRecord.DisplayType))
                   : layoutComponentResults.Select(x => x.Properties);

            return Shape.Grid(Id: gridId, Horizontal: horizontal, Columns: columns, Items: shapes, Classes: new [] { gridClass }, RowClasses: new [] { rowClass });
        }
    }
}