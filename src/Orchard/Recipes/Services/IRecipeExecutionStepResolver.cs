using System.Collections.Generic;

namespace Orchard.Recipes.Services
{
    public interface IRecipeExecutionStepResolver :IDependency
    {
        IRecipeExecutionStep Resolve(string importStepName);
        IEnumerable<IRecipeExecutionStep> Resolve(IEnumerable<string> exportStepNames);
    }
}