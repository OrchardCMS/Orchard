using System.Xml.Linq;
using Orchard.Environment.Descriptor;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public class RecipeExecutor : Component, IRecipeExecutor {
        private readonly IRecipeParser _recipeParser;
        private readonly IRecipeManager _recipeManager;
        private readonly IShellDescriptorManager _shellDescriptorManager;

        public RecipeExecutor(
            IRecipeParser recipeParser, 
            IRecipeManager recipeManager,
            IShellDescriptorManager shellDescriptorManager) {
            
            _recipeParser = recipeParser;
            _recipeManager = recipeManager;
            _shellDescriptorManager = shellDescriptorManager;
        }

        public string Execute(XDocument recipeDocument) {
            var recipeText = recipeDocument.ToString();
            return Execute(recipeText);
        }

        public string Execute(string recipeText) {
            var recipe = _recipeParser.ParseRecipe(recipeText);
            return Execute(recipe);
        }

        public string Execute(Recipe recipe) {
            var executionId = _recipeManager.Execute(recipe);
            UpdateShell();
            return executionId;
        }

        private void UpdateShell() {
            var descriptor = _shellDescriptorManager.GetShellDescriptor();
            _shellDescriptorManager.UpdateShellDescriptor(descriptor.SerialNumber, descriptor.Features, descriptor.Parameters);
        }
    }
}