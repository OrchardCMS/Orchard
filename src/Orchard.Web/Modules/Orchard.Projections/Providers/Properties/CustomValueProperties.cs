using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Property;
using Orchard.Projections.Services;

namespace Orchard.Projections.Providers.Properties {
    public class CustomValueProperty : IPropertyProvider {
        protected dynamic Shape { get; set; }

        public CustomValueProperty(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribePropertyContext describe) {
            describe.For("Content", T("Content"),T("Content properties"))
                .Element("CustomValue", T("Custom Value"), T("A static text. Use it for custom tokens with Rewrite options."),
                    DisplayProperty,
                    RenderProperty
                );
        }

        public LocalizedString DisplayProperty(PropertyContext context) {
            return T("Content: Custom Value");
        }

        public dynamic RenderProperty(PropertyContext context, ContentItem contentItem) {
            // don't return empty otherwise the layout won't render the rewritten value
            return " ";
        }
    }
}