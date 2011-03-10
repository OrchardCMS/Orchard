using System;
using JetBrains.Annotations;
using Orchard.Environment.Descriptor;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.ImportExport.Services {
    [UsedImplicitly]
    public class ImportExportService : IImportExportService {
        private readonly IRecipeParser _recipeParser;
        private readonly IRecipeManager _recipeManager;
        private readonly IShellDescriptorManager _shellDescriptorManager;

        public ImportExportService(IRecipeParser recipeParser, IRecipeManager recipeManager, IShellDescriptorManager shellDescriptorManager) {
            _recipeParser = recipeParser;
            _recipeManager = recipeManager;
            _shellDescriptorManager = shellDescriptorManager;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public void Import(string recipeText) {
            var recipe = _recipeParser.ParseRecipe(recipeText);
            CheckRecipeSteps(recipe);
            _recipeManager.Execute(recipe);
            UpdateShell();
        }

        private void CheckRecipeSteps(Recipe recipe) {
            foreach (var step in recipe.RecipeSteps) {
                switch (step.Name) {
                    case "Metadata":
                    case "Settings":
                    case "Data":
                        break;
                    default:
                        throw new InvalidOperationException(T("Step {0} is not a supported import step.", step.Name).Text);
                }
            }
        }

        private void UpdateShell() {
            var descriptor = _shellDescriptorManager.GetShellDescriptor();
            _shellDescriptorManager.UpdateShellDescriptor(descriptor.SerialNumber, descriptor.Features, descriptor.Parameters);
        }
    }
}