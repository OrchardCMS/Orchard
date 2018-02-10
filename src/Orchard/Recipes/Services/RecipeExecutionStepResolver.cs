using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.Recipes.Services
{
    public class RecipeExecutionStepResolver : IRecipeExecutionStepResolver
    {
        private readonly IEnumerable<IRecipeExecutionStep> _recipeExecutionSteps;

        public RecipeExecutionStepResolver(IEnumerable<IRecipeExecutionStep> recipeExecutionSteps) {
            _recipeExecutionSteps = recipeExecutionSteps;
        }

        public IRecipeExecutionStep Resolve(string importStepName) {
           return _recipeExecutionSteps.SingleOrDefault(x => x.Names.Contains(importStepName));
        }

        public IEnumerable<IRecipeExecutionStep> Resolve(IEnumerable<string> importStepNames) {
            return from name in importStepNames
                let provider = _recipeExecutionSteps.SingleOrDefault(x => x.Names.Contains(name))
                where provider != null
                select provider;
        }
    }
}
