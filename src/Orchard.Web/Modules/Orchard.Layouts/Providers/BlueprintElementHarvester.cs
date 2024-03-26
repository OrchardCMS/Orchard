using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Environment;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Framework.Harvesters;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Models;
using Orchard.Layouts.Services;

namespace Orchard.Layouts.Providers {
    public class BlueprintElementHarvester : Component, IElementHarvester {
        private readonly Work<IElementBlueprintService> _elementBlueprintService;
        private readonly Work<IElementManager> _elementManager;

        public BlueprintElementHarvester(Work<IElementBlueprintService> elementBlueprintService, Work<IElementManager> elementManager) {
            _elementBlueprintService = elementBlueprintService;
            _elementManager = elementManager;
        }

        public IEnumerable<ElementDescriptor> HarvestElements(HarvestElementsContext context) {
            if (context.IsHarvesting)
                return Enumerable.Empty<ElementDescriptor>();

            var blueprints = _elementBlueprintService.Value.GetBlueprints().ToArray();

            var query =
                from blueprint in blueprints
                let describeContext = new DescribeElementsContext {Content = context.Content, CacheVaryParam = "Blueprints", IsHarvesting = true }
                let baseElementDescriptor = _elementManager.Value.GetElementDescriptorByTypeName(describeContext, blueprint.BaseElementTypeName)
                let baseElement = _elementManager.Value.ActivateElement(baseElementDescriptor)
                select new ElementDescriptor(
                    baseElement.Descriptor.ElementType,
                    blueprint.ElementTypeName,
                    T(blueprint.ElementDisplayName),
                    T(!String.IsNullOrWhiteSpace(blueprint.ElementDescription) ? blueprint.ElementDescription : blueprint.ElementDisplayName),
                    GetCategory(blueprint)) {
                        EnableEditorDialog = false,
                        IsSystemElement = false,
                        CreatingDisplay = creatingDisplayContext => CreatingDisplay(creatingDisplayContext, blueprint),
                        Displaying = displayContext => Displaying(displayContext, baseElement),
                        StateBag = new Dictionary<string, object> {
                            {"Blueprint", true},
                            {"ElementTypeName", baseElement.Descriptor.TypeName}
                        }
                    };

            return query.ToArray();
        }

        private static string GetCategory(ElementBlueprint blueprint) {
            return !String.IsNullOrWhiteSpace(blueprint.ElementCategory) ? blueprint.ElementCategory : "Blueprints";
        }

        private void CreatingDisplay(ElementCreatingDisplayShapeContext context, ElementBlueprint blueprint) {
            var bluePrintState = ElementDataHelper.Deserialize(blueprint.BaseElementState);
            context.Element.Data = bluePrintState;
        }

        private void Displaying(ElementDisplayingContext context, Element element) {
            var drivers = _elementManager.Value.GetDrivers(element);

            foreach (var driver in drivers) {
                driver.Displaying(context);
            }
        }
    }
}