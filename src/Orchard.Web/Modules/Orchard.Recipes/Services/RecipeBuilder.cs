using System.Collections.Generic;
using System.Xml.Linq;

namespace Orchard.Recipes.Services {
    public class RecipeBuilder : Component, IRecipeBuilder {
        
        public string Build(IEnumerable<IRecipeBuilderStep> steps) {
            var exportDocument = CreateRecipeRoot();
            var context = new BuildContext {
                RecipeDocument = CreateRecipeRoot()
            };
            
            foreach (var step in steps) {
                step.Build(context);
            }
            
            return exportDocument.ToString();
        }

        private XDocument CreateRecipeRoot() {
            var recipeRoot = new XDocument(
                new XDeclaration("1.0", "", "yes"),
                new XComment("Exported from Orchard"),
                new XElement("Orchard",
                    new XElement("Recipe")
                )
            );
            return recipeRoot;
        }
    }
}