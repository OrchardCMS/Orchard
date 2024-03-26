using System.Collections.Generic;
using System.Linq;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.Providers.RecipeHandlers {
    /// <summary>
    /// Delegates execution of the step to the appropriate recipe execution step implementation.
    /// </summary>
    public class RecipeExecutionStepHandler : Component, IRecipeHandler {
        private readonly IRecipeExecutionStepResolver _recipeExecutionStepResolver;
        public RecipeExecutionStepHandler(IRecipeExecutionStepResolver recipeExecutionStepResolver) {
            _recipeExecutionStepResolver = recipeExecutionStepResolver;
        }

        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            var executionStep = _recipeExecutionStepResolver.Resolve(recipeContext.RecipeStep.Name);
            var recipeExecutionContext = new RecipeExecutionContext {ExecutionId = recipeContext.ExecutionId, RecipeStep = recipeContext.RecipeStep};

            if (executionStep != null) {
                Logger.Information("Executing recipe step '{0}'.", recipeContext.RecipeStep.Name);
                executionStep.Execute(recipeExecutionContext);
                Logger.Information("Finished executing recipe step '{0}'.", recipeContext.RecipeStep.Name);
                recipeContext.Executed = true;
            }
        }
    }
}