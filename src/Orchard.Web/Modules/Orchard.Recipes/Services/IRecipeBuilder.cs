using System.Collections.Generic;

namespace Orchard.Recipes.Services {
    public interface IRecipeBuilder : IDependency {
        string Build(IEnumerable<IRecipeBuilderStep> steps);
    }
}