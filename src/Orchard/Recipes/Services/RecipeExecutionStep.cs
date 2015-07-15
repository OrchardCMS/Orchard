namespace Orchard.Recipes.Services {
    public abstract class RecipeExecutionStep : Component, IRecipeExecutionStep {
        public abstract string Name { get; }
        public abstract void Execute(RecipeExecutionContext context);
    }
}