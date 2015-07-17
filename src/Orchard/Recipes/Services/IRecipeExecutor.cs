using System.Xml.Linq;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public interface IRecipeExecutor : IDependency {
        string Execute(Recipe recipe);
    }
}