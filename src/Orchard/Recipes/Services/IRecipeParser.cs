using System.Xml.Linq;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public interface IRecipeParser : IDependency {
        Recipe ParseRecipe(XDocument recipeDocument);
        Recipe ParseRecipe(string recipeText);
    }
}
