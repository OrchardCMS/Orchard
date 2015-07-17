using System.Linq;
using System.Xml.Linq;
using Orchard.Data;
using Orchard.Layouts.Models;
using Orchard.Localization;
using Orchard.Recipes.Services;

namespace Orchard.Layouts.Recipes.Builders {

    public class CustomElementsStep : RecipeBuilderStep {
        private readonly IRepository<ElementBlueprint> _repository;

        public CustomElementsStep(IRepository<ElementBlueprint> repository) {
            _repository = repository;
        }

        public override string Name {
            get { return "CustomElements"; }
        }

        public override LocalizedString DisplayName {
            get { return T("Custom Elements"); }
        }

        public override LocalizedString Description {
            get { return T("Exports custom defined elements."); }
        }

        public override void Build(BuildContext context) {
            var elements = _repository.Table.OrderBy(x => x.ElementTypeName).ToList();

            if (!elements.Any()) {
                return;
            }

            var root = new XElement("CustomElements");
            context.RecipeDocument.Element("Orchard").Add(root);

            foreach (var element in elements) {
                root.Add(new XElement("Element",
                    new XAttribute("ElementTypeName", element.ElementTypeName),
                    new XAttribute("BaseElementTypeName", element.BaseElementTypeName),
                    new XAttribute("ElementDisplayName", element.ElementDisplayName),
                    new XAttribute("ElementDescription", element.ElementDescription),
                    new XAttribute("ElementCategory", element.ElementCategory),
                    new XElement("BaseElementState", new XCData(element.BaseElementState))));
            }
        }
    }
}

