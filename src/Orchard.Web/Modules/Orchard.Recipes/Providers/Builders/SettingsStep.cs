using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.Providers.Builders {
    public class SettingsStep : RecipeBuilderStep {
        private readonly IOrchardServices _orchardServices;

        public SettingsStep(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public override string Name {
            get { return "Settings"; }
        }

        public override LocalizedString DisplayName {
            get { return T("Settings"); }
        }

        public override LocalizedString Description {
            get { return T("Exports settings. Please verify that you are not exporting confidential information, such as passwords or application keys."); }
        }

        public override int Priority { get { return 30; } }
        public override int Position { get { return 60; } }

        public override void Build(BuildContext context) {
            context.RecipeDocument.Element("Orchard").Add(ExportSiteSettings());
        }

        private XElement ExportSiteSettings() {
            var siteContentItem = _orchardServices.WorkContext.CurrentSite.ContentItem;
            var exportedElements = ExportContentItem(siteContentItem).Elements().ToList();

            foreach (var contentPart in siteContentItem.Parts.OrderBy(x => x.PartDefinition.Name)) {
                var exportedElement = exportedElements.FirstOrDefault(element => element.Name == contentPart.PartDefinition.Name);

                // Get all simple attributes if exported element is null.
                // Get exclude the simple attributes that already exist if element is not null.
                var simpleAttributes =
                    ExportSettingsPartAttributes(contentPart)
                    .Where(attribute => exportedElement == null || exportedElement.Attributes().All(xAttribute => xAttribute.Name != attribute.Name))
                    .OrderBy(x => x.Name.LocalName)
                    .ToList();

                if (simpleAttributes.Any()) {
                    if (exportedElement == null) {
                        exportedElement = new XElement(contentPart.PartDefinition.Name);
                        exportedElements.Add(exportedElement);
                    }

                    exportedElement.Add(simpleAttributes);
                }
            }

            exportedElements = exportedElements.OrderBy(x => x.Name.LocalName).ToList();
            return new XElement("Settings", exportedElements);
        }

        private XElement ExportContentItem(ContentItem contentItem) {
            return _orchardServices.ContentManager.Export(contentItem);
        }

        private IEnumerable<XAttribute> ExportSettingsPartAttributes(ContentPart sitePart) {
            foreach (var property in sitePart.GetType().GetProperties().OrderBy(x => x.Name)) {
                var propertyType = property.PropertyType;

                // Supported types (we also know they are not indexed properties).
                if (propertyType == typeof(string) || propertyType == typeof(bool) || propertyType == typeof(int)) {
                    // Exclude read-only properties.
                    if (property.GetSetMethod() != null) {
                        var value = property.GetValue(sitePart, null);
                        if (value == null)
                            continue;

                        yield return new XAttribute(property.Name, value);
                    }
                }
            }
        }
    }
}