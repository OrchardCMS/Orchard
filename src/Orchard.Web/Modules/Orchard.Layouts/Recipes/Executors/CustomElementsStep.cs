using System;
using System.Collections.Generic;
using Orchard.Data;
using Orchard.Layouts.Models;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Layouts.Services;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Layouts.Framework.Drivers;

namespace Orchard.Layouts.Recipes.Executors {
    public class CustomElementsStep : RecipeExecutionStep {
        private readonly IRepository<ElementBlueprint> _repository;
        private readonly IElementManager _elementManager;
        private readonly IOrchardServices _orchardServices;

        public CustomElementsStep(
            IRepository<ElementBlueprint> repository,
            IElementManager elementManager,
            IOrchardServices orchardServices,
            RecipeExecutionLogger logger) : base(logger) {

            _repository = repository;
            _elementManager = elementManager;
            _orchardServices = orchardServices;
        }

        public override string Name {
            get { return "CustomElements"; }
        }

        public override IEnumerable<string> Names {
            get { return new[] { Name, "LayoutElements" }; }
        }

        public override void Execute(RecipeExecutionContext context)
        {

            var blueprintEntries = new List<Tuple<ElementBlueprint, Element>>();

            foreach (var blueprintElement in context.RecipeStep.Step.Elements())
            {
                var typeName = blueprintElement.Attribute("ElementTypeName").Value;
                Logger.Information("Importing custom element '{0}'.", typeName);

                try
                {
                    var blueprint = GetOrCreateElement(typeName);
                    blueprint.BaseElementTypeName = blueprintElement.Attribute("BaseElementTypeName").Value;
                    blueprint.ElementDisplayName = blueprintElement.Attribute("ElementDisplayName").Value;
                    blueprint.ElementDescription = (string)blueprintElement.Attribute("ElementDescription");
                    blueprint.ElementCategory = (string)blueprintElement.Attribute("ElementCategory");
                    blueprint.BaseElementState = blueprintElement.Element("BaseElementState").Value;

                    var describeContext = DescribeElementsContext.Empty;
                    var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, blueprint.BaseElementTypeName);
                    var baseElement = _elementManager.ActivateElement(descriptor);
                    baseElement.Data = ElementDataHelper.Deserialize(blueprint.BaseElementState);
                    baseElement.ExportableData = ElementDataHelper.Deserialize(blueprintElement.Attribute("BaseExportableData").Value);

                    blueprintEntries.Add(new Tuple<ElementBlueprint, Element>(blueprint, baseElement));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error while importing custom element '{0}'.", typeName);
                    throw;
                }
            }

            var baseElements = blueprintEntries.Select(e => e.Item2).ToList();

            var importContentSession = new ImportContentSession(_orchardServices.ContentManager);
            var importLayoutContext = new ImportLayoutContext
            {
                Session = new ImportContentSessionWrapper(importContentSession)
            };

            _elementManager.Importing(baseElements, importLayoutContext);
            _elementManager.Imported(baseElements, importLayoutContext);
            _elementManager.ImportCompleted(baseElements, importLayoutContext);

            foreach (var blueprintEntry in blueprintEntries)
                blueprintEntry.Item1.BaseElementState = blueprintEntry.Item2.Data.Serialize();

        }

        private ElementBlueprint GetOrCreateElement(string typeName) {
            var element = _repository.Get(x => x.ElementTypeName == typeName);

            if (element == null) {
                element = new ElementBlueprint {
                    ElementTypeName = typeName
                };
                _repository.Create(element);
            }

            return element;
        }
    }
}
