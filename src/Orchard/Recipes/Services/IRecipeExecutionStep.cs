using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public interface IRecipeExecutionStep : IDependency {
        string Name { get; }
        LocalizedString DisplayName { get; }
        LocalizedString Description { get; }
        dynamic BuildEditor(dynamic shapeFactory);
        dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater);
        void UpdateStep(UpdateRecipeExecutionStepContext context);
        void Execute(RecipeExecutionContext context);
        
    }
}