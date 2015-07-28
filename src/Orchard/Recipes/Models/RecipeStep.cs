using System.Xml.Linq;

namespace Orchard.Recipes.Models {
    public class RecipeStep {
        public RecipeStep(string recipeName, string name, XElement step) {
            RecipeName = recipeName;
            Name = name;
            Step = step;
        }

        public string RecipeName { get; private set; }
        public string Name { get; private set; }
        public XElement Step { get; private set; }
    }
}