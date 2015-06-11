using System.Collections.Generic;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public interface IRecipeHarvester : IDependency {
        IEnumerable<Recipe> HarvestRecipes(string extensionId);
    }
}
