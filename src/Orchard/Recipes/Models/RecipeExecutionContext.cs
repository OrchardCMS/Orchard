using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public class RecipeExecutionContext {
        public string ExecutionId { get; set; }
        public RecipeStep RecipeStep { get; set; }
    }
}