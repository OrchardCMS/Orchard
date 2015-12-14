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
    public class ShapeLayout : ILayoutProvider {
        private readonly IContentManager _contentManager;
        protected dynamic Shape { get; set; }

        public ShapeLayout(IShapeFactory shapeFactory, IContentManager contentManager) {
            _contentManager = contentManager;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeLayoutContext describe) {
            describe.For("Html", T("Html"),T("Html Layouts"))
                .Element("Shape", T("Shape"), T("Uses a specific shape name to render the layout."),
                    DisplayLayout,
                    RenderLayout,
                    "ShapeLayout"
                );
        }

        public LocalizedString DisplayLayout(LayoutContext context) {
            return T("Renders content in a {0} layout", context.State.ShapeType.ToString());
        }

        public dynamic RenderLayout(LayoutContext context, IEnumerable<LayoutComponentResult> layoutComponentResults) {
            string shapeType = context.State.ShapeType;

            dynamic shape = ((IShapeFactory) Shape).Create(shapeType);
            shape.ContentItems = layoutComponentResults.Select(x => x.ContentItem);
            shape.BuildShapes= (Func<IEnumerable<dynamic>>) (() => context.LayoutRecord.Display == (int)LayoutRecord.Displays.Content
                   ? layoutComponentResults.Select(x => _contentManager.BuildDisplay(x.ContentItem, context.LayoutRecord.DisplayType))
                   : layoutComponentResults.Select(x => x.Properties));

            
            return shape;
        }
    }
}