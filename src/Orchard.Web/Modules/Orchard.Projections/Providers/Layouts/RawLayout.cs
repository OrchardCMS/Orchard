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
    public class RawLayout : ILayoutProvider {
        private readonly IContentManager _contentManager;
        protected dynamic Shape { get; set; }

        public RawLayout(IShapeFactory shapeFactory, IContentManager contentManager) {
            _contentManager = contentManager;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeLayoutContext describe) {
            describe.For("Html", T("Html"),T("Html Layouts"))
                .Element("Raw", T("Raw"), T("Renders content with custom separators."),
                    DisplayLayout,
                    RenderLayout,
                    "RawLayout"
                );
        }

        public LocalizedString DisplayLayout(LayoutContext context) {
            return T("Renders content with custom separators.");
        }

        public dynamic RenderLayout(LayoutContext context, IEnumerable<LayoutComponentResult> layoutComponentResults) {
            string containerTag = context.State.ContainerTag;
            string containerId = context.State.ContainerId;
            string containerClass = context.State.ContainerClass;
            if (!String.IsNullOrEmpty(containerClass)) containerClass += " ";
            containerClass += "projector-layout projector-raw-layout";

            string itemTag = context.State.ItemTag;
            string itemClass = context.State.ItemClass;

            string prepend = context.State.Prepend;
            string append = context.State.Append;
            string separator = context.State.Separator;

            IEnumerable<dynamic> shapes =
               context.LayoutRecord.Display == (int)LayoutRecord.Displays.Content
                   ? layoutComponentResults.Select(x => _contentManager.BuildDisplay(x.ContentItem, context.LayoutRecord.DisplayType))
                   : layoutComponentResults.Select(x => x.Properties);

            return Shape.Raw(
                Id: containerId, 
                Items: shapes, 
                Tag: containerTag,
                Classes: new [] { containerClass },
                ItemTag: itemTag,
                ItemClasses: new [] { itemClass },
                Prepend: prepend,
                Append: append,
                Separator: separator
                );
        }
    }
}