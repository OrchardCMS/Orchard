using Orchard.Recipes.Models;

namespace Orchard.ImportExport.Services {
    public interface IRecipeSerializer : IDependency {
        string Serialize(Recipe recipe);
    }
}