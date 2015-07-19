namespace Orchard.Recipes.ViewModels {
    public class SetupRecipeStepViewModel {
        public string RecipeName { get; set; }
        public string RecipeDescription { get; set; }
        public string RecipeAuthor { get; set; }
        public string RecipeWebsite { get; set; }
        public string RecipeTags { get; set; }
        public string RecipeCategory { get; set; }
        public string RecipeVersion { get; set; }
        public bool IsSetupRecipe { get; set; }
    }
}