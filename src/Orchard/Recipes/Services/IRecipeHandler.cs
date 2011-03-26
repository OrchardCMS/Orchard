using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public interface IRecipeHandler : IDependency {
        void ExecuteRecipeStep(RecipeContext recipeContext);
    }
}
