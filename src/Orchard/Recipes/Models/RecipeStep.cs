using System.Xml.Linq;

namespace Orchard.Recipes.Models {
    public class RecipeStep {
        public string Name { get; set; }
        public XElement Step { get; set; }
    }
}