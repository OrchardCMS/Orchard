using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public interface IRecipeStepQueue : ISingletonDependency {
        void Enqueue(string executionId, RecipeStep step);
        RecipeStep Dequeue(string executionId);
    }
}
