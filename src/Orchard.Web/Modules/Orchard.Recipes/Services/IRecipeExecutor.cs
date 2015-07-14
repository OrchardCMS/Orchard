namespace Orchard.Recipes.Services {
    public interface IRecipeExecutor : IDependency {
        string Execute(string recipeText);
    }
}