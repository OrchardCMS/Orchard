using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Framework.Harvesters;
using Orchard.Layouts.Services;
using Orchard.Themes.Services;
using Orchard.Utility.Extensions;

namespace Orchard.Layouts.Providers {
    [OrchardFeature("Orchard.Layouts.Snippets")]
    public class SnippetElementHarvester : Component, ElementHarvester {
        private const string SnippetShapeSuffix = "Snippet";
        private readonly Work<IShapeFactory> _shapeFactory;
        private readonly Work<ISiteThemeService> _siteThemeService;
        private readonly Work<IShapeTableLocator> _shapeTableLocator;
        private readonly Work<IElementFactory> _elementFactory;

        public SnippetElementHarvester(
            IWorkContextAccessor workContextAccessor,
            Work<IShapeFactory> shapeFactory,
            Work<ISiteThemeService> siteThemeService,
            Work<IShapeTableLocator> shapeTableLocator, 
            Work<IElementFactory> elementFactory) {

            _shapeFactory = shapeFactory;
            _siteThemeService = siteThemeService;
            _shapeTableLocator = shapeTableLocator;
            _elementFactory = elementFactory;
            workContextAccessor.GetContext();
        }

        public IEnumerable<ElementDescriptor> HarvestElements(HarvestElementsContext context) {
            var currentThemeName = _siteThemeService.Value.GetCurrentThemeName();
            var shapeTable = _shapeTableLocator.Value.Lookup(currentThemeName);
            var shapeDescriptors = shapeTable.Bindings.Where(x => !String.Equals(x.Key, "Elements_Snippet", StringComparison.OrdinalIgnoreCase) && x.Key.EndsWith(SnippetShapeSuffix, StringComparison.OrdinalIgnoreCase)).ToDictionary(x => x.Key, x => x.Value.ShapeDescriptor);
            var elementType = typeof (Snippet);
            var snippetElement = _elementFactory.Value.Activate(elementType);

            foreach (var shapeDescriptor in shapeDescriptors) {
                var shapeType = shapeDescriptor.Value.ShapeType;
                var elementName = GetDisplayName(shapeDescriptor.Value.BindingSource);
                var closureDescriptor = shapeDescriptor;
                yield return new ElementDescriptor(elementType, shapeType, T(elementName), T("An element that renders the {0} shape.", shapeType), snippetElement.Category) {
                    Display = displayContext => Displaying(displayContext, closureDescriptor.Value),
                    ToolboxIcon = "\uf10c"
                };
            }
        }

        private void Displaying(ElementDisplayContext context, ShapeDescriptor shapeDescriptor) {
            var shapeType = shapeDescriptor.ShapeType;
            var shape = _shapeFactory.Value.Create(shapeType);
            context.ElementShape.Snippet = shape;
        }

        private string GetDisplayName(string bindingSource) {
            var fileName = Path.GetFileNameWithoutExtension(bindingSource);
            var lastIndex = fileName.IndexOf(SnippetShapeSuffix, StringComparison.OrdinalIgnoreCase);
            return fileName.Substring(0, lastIndex).CamelFriendly();
        }
    }
}