using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Property;
using Orchard.Projections.Services;

namespace Orchard.Projections.Providers.Properties {
    public class ContentProperties : IPropertyProvider {
        private readonly IContentManager _contentManager;
        protected dynamic Shape { get; set; }

        public ContentProperties(IShapeFactory shapeFactory, IContentManager contentManager) {
            _contentManager = contentManager;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribePropertyContext describe) {
            describe.For("Content", T("Content"),T("Content properties"))
                .Element("Metadata:DisplayText", T("Display Text"), T("The text representing the content, e.g. its Title."),
                    DisplayProperty,
                    RenderProperty
                );
        }

        public LocalizedString DisplayProperty(PropertyContext context) {
            return T("Content: Display Text");
        }

        public dynamic RenderProperty(PropertyContext context, ContentItem contentItem) {
            return _contentManager.GetItemMetadata(contentItem).DisplayText;
        }
    }
}