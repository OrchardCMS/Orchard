namespace Orchard.Recipes.Services {
    public interface IRecipeExecutionStep : IDependency {
        string Name { get; }
        void Execute(RecipeExecutionContext context);
    }
}