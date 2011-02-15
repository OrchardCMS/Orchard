using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public interface IRecipeManager : IDependency {
        string Execute(Recipe recipe);
    }
}
