using Orchard.Commands;
using Orchard.Environment.Extensions;
using Orchard.Recipes.Services;
using System;
using System.Linq;

namespace Orchard.Recipes.Commands {
    public class RecipesCommands : DefaultOrchardCommandHandler {
        private readonly IRecipeHarvester _recipeHarvester;
        private readonly IRecipeManager _recipeManager;
        private readonly IExtensionManager _extensionManager;
        private readonly IRecipeResultAccessor _recipeResultAccessor;

        public RecipesCommands(
            IRecipeHarvester recipeHarvester,
            IRecipeManager recipeManager,
            IExtensionManager extensionManager,
            IRecipeResultAccessor recipeResultAccessor) {
            _recipeHarvester = recipeHarvester;
            _recipeManager = recipeManager;
            _extensionManager = extensionManager;
            _recipeResultAccessor = recipeResultAccessor;
        }

        [CommandHelp("recipes harvest <extensionId>\r\n\t" + "Displays a list of available recipes for a specific extension.")]
        [CommandName("recipes harvest")]
        public void Harvest(string extensionId) {
            var extensionDescriptor = _extensionManager.GetExtension(extensionId);
            if (extensionDescriptor == null) {
                Context.Output.WriteLine(T("Could not discover recipes because extension '{0}' was not found.", extensionId));
                return;            
            }

            var recipes = _recipeHarvester.HarvestRecipes(extensionId);
            if (!recipes.Any()) {
                Context.Output.WriteLine(T("No recipes found for extension '{0}'.", extensionId));
                return;
            }

            Context.Output.WriteLine(T("List of available recipes"));
            Context.Output.WriteLine(T("--------------------------"));
            Context.Output.WriteLine();

            foreach (var recipe in recipes) {
                Context.Output.WriteLine(T("Recipe: {0}", recipe.Name));
                Context.Output.WriteLine(T("  Version:     {0}", recipe.Version));
                Context.Output.WriteLine(T("  Tags:        {0}", recipe.Tags));
                Context.Output.WriteLine(T("  Description: {0}", recipe.Description));
                Context.Output.WriteLine(T("  Author:      {0}", recipe.Author));
                Context.Output.WriteLine(T("  Website:     {0}", recipe.WebSite));
            }
        }

        [CommandHelp("recipes execute <extensionId> <recipe>\r\n\t" + "Executes a specific recipe for a specific extension.")]
        [CommandName("recipes execute")]
        public void Execute(string extensionId, string recipeName) {
            var extensionDescriptor = _extensionManager.GetExtension(extensionId);
            if (extensionDescriptor == null) {
                Context.Output.WriteLine(T("Could not discover recipes because extension '{0}' was not found.", extensionId));
                return;
            }

            var recipes = _recipeHarvester.HarvestRecipes(extensionId);
            if (!recipes.Any()) {
                Context.Output.WriteLine(T("No recipes found for extension '{0}'.", extensionId));
                return;
            }

            var recipe = recipes.FirstOrDefault(r => r.Name.Equals(recipeName, StringComparison.OrdinalIgnoreCase));
            if (recipe == null) {
                Context.Output.WriteLine(T("No recipe with name '{0}' was found in extension '{1}'.", recipeName, extensionId));
                return;
            }

            var executionId = _recipeManager.Execute(recipe);
            Context.Output.WriteLine(T("Recipe successfully scheduled with execution ID {0}. Use the 'recipes result' command to check the result of the execution.", executionId).Text);
        }

        [CommandHelp("recipes result <executionId>\r\n\t" + "Prints the status/result of a specific recipe execution.")]
        [CommandName("recipes result")]
        public void Result(string executionId) {
            var result = _recipeResultAccessor.GetResult(executionId);

            Context.Output.WriteLine(T("Result of recipe execution:"));
            Context.Output.WriteLine(T("---------------------------"));
            Context.Output.WriteLine(T("  Execution ID: {0}", executionId));
            Context.Output.WriteLine(T("  Completed:    {0}", result.IsCompleted));
            Context.Output.WriteLine(T("  Successful:   {0}", result.IsSuccessful));

            foreach (var step in result.Steps) {
                Context.Output.WriteLine(T("  Step: {0} ({1})", step.StepName, step.RecipeName));
                Context.Output.WriteLine(T("    Completed:     {0}", step.IsCompleted));
                Context.Output.WriteLine(T("    Successful:    {0}", step.IsSuccessful));
                if (!String.IsNullOrEmpty(step.ErrorMessage))
                    Context.Output.WriteLine(T("    Error message: {0}", step.ErrorMessage));
            }
        }
    }
}