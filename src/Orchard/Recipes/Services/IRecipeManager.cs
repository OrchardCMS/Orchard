using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public interface IRecipeManager : IDependency {
        void Execute(Recipe recipe);
    }
}
