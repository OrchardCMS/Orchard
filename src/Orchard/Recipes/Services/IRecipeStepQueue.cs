using System;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public interface IRecipeStepQueue : IDependency {
        void Enqueue(RecipeStep step, string executionId);
        Tuple<RecipeStep, string> Dequeue();
    }
}
