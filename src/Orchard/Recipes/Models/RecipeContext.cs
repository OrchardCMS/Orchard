namespace Orchard.Recipes.Models {
    public class RecipeContext {
        public Recipe Recipe { get; set; }
        public RecipeStep RecipeStep { get; set; }
        public bool Executed { get; set; }
    }
}