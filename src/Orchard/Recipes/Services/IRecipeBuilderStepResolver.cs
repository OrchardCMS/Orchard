using System.Collections.Generic;

namespace Orchard.Recipes.Services
{
    public interface IRecipeBuilderStepResolver : IDependency
    {
        IRecipeBuilderStep Resolve(string exportStepName);
        IEnumerable<IRecipeBuilderStep> Resolve(IEnumerable<string> exportStepNames);
    }
}