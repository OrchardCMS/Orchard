using System;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public interface IRecipeStepQueue : ISingletonDependency {
        void Enqueue(string executionId, RecipeStep step);
        Tuple<RecipeStep, string> Dequeue(string executionId);
    }
}
