using System;
using Orchard.Data;
using Orchard.Layouts.Models;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Layouts.ImportExport {
    public class ElementsImportHandler : Component, IRecipeHandler {
        private readonly IRepository<ElementBlueprint> _repository;

        public ElementsImportHandler(IRepository<ElementBlueprint> repository) {
            _repository = repository;
        }

        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "LayoutElements", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            foreach (var elementElement in recipeContext.RecipeStep.Step.Elements()) {
                var typeName = elementElement.Attribute("ElementTypeName").Value;
                var element = GetOrCreateElement(typeName);

                element.BaseElementTypeName = elementElement.Attribute("BaseElementTypeName").Value;
                element.ElementDisplayName = elementElement.Attribute("ElementDisplayName").Value;
                element.ElementDescription = elementElement.Attribute("ElementDescription").Value;
                element.ElementCategory = elementElement.Attribute("ElementCategory").Value;
                element.BaseElementState = elementElement.Element("BaseElementState").Value;
            }

            recipeContext.Executed = true;
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
