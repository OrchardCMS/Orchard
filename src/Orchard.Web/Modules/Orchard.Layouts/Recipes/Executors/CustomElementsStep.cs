using System;
using System.Collections.Generic;
using Orchard.Data;
using Orchard.Layouts.Models;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Layouts.Recipes.Executors {
    public class CustomElementsStep : RecipeExecutionStep {
        private readonly IRepository<ElementBlueprint> _repository;

        public CustomElementsStep(
            IRepository<ElementBlueprint> repository,
            RecipeExecutionLogger logger) : base(logger) {

            _repository = repository;
        }

        public override string Name {
            get { return "CustomElements"; }
        }

        public override IEnumerable<string> Names {
            get { return new[] { Name, "LayoutElements" }; }
        }

        public override void Execute(RecipeExecutionContext context) {
            foreach (var elementElement in context.RecipeStep.Step.Elements()) {
                var typeName = elementElement.Attribute("ElementTypeName").Value;
                Logger.Information("Importing custom element '{0}'.", typeName);

                try {
                    var element = GetOrCreateElement(typeName);
                    element.BaseElementTypeName = elementElement.Attribute("BaseElementTypeName").Value;
                    element.ElementDisplayName = elementElement.Attribute("ElementDisplayName").Value;
                    element.ElementDescription = elementElement.Attribute("ElementDescription").Value;
                    element.ElementCategory = elementElement.Attribute("ElementCategory").Value;
                    element.BaseElementState = elementElement.Element("BaseElementState").Value;
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Error while importing custom element '{0}'.", typeName);
                    throw;
                }
            }
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
