using System;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public interface IRecipeStepQueue : ISingletonDependency {
        void Enqueue(RecipeStep step, string executionId);
        Tuple<RecipeStep, string> Dequeue();
    }
}
