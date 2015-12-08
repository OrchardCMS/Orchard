using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public interface IRecipeParser : IDependency {
        Recipe ParseRecipe(string recipeText);
    }
}
