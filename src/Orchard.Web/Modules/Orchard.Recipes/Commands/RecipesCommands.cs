using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Commands;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.Commands {
    public class RecipesCommands : DefaultOrchardCommandHandler {
        private readonly IRecipeHarvester _recipeHarvester;
        private readonly IRecipeManager _recipeManager;
        private readonly IExtensionManager _extensionManager;

        public RecipesCommands(IRecipeHarvester recipeHarvester, IRecipeManager recipeManager, IExtensionManager extensionManager) {
            _recipeHarvester = recipeHarvester;
            _recipeManager = recipeManager;
            _extensionManager = extensionManager;
        }

        [CommandHelp("recipes harvest <extension-id>\r\n\t" + "Display list of available recipes for an extension")]
        [CommandName("recipes harvest")]
        public void HarvestRecipes(string extensionId) {
            ExtensionDescriptor extensionDescriptor = _extensionManager.GetExtension(extensionId);
            if (extensionDescriptor == null) {
                Context.Output.WriteLine(T("Could not discover recipes because module '{0}' was not found.", extensionId));
                return;            
            }

            IEnumerable<Recipe> recipes = _recipeHarvester.HarvestRecipes(extensionId);
            if (recipes.Count() == 0) {
                Context.Output.WriteLine(T("No recipes found for extension {0}.", extensionId));
                return;
            }

            Context.Output.WriteLine(T("List of available recipes"));
            Context.Output.WriteLine(T("--------------------------"));
            Context.Output.WriteLine();

            foreach (Recipe recipe in recipes) {
                Context.Output.WriteLine(T("Recipe: {0}", recipe.Name));
                Context.Output.WriteLine(T("  Version:     {0}", recipe.Version));
                Context.Output.WriteLine(T("  Tags:        {0}", recipe.Tags));
                Context.Output.WriteLine(T("  Description: {0}", recipe.Description));
                Context.Output.WriteLine(T("  Author:      {0}", recipe.Author));
                Context.Output.WriteLine(T("  Website:     {0}", recipe.WebSite));
            }
        }

        [CommandHelp("recipes execute <extension-id> <recipe-name>\r\n\t" + "Executes a recipe from a module")]
        [CommandName("recipes execute")]
        public void ExecuteRecipe(string extensionId, string recipeName) {
            ExtensionDescriptor extensionDescriptor = _extensionManager.GetExtension(extensionId);
            if (extensionDescriptor == null) {
                Context.Output.WriteLine(T("Could not discover recipes because module '{0}' was not found.", extensionId));
                return;
            }

            IEnumerable<Recipe> recipes = _recipeHarvester.HarvestRecipes(extensionId);
            if (recipes.Count() == 0) {
                Context.Output.WriteLine(T("No recipes found for extension {0}.", extensionId));
                return;
            }

            Recipe recipe = recipes.FirstOrDefault(r => r.Name.Equals(recipeName, StringComparison.OrdinalIgnoreCase));
            if (recipe == null) {
                Context.Output.WriteLine(T("Invalid recipe name {0}.", recipeName));
                return;
            }

            _recipeManager.Execute(recipe);
            Context.Output.WriteLine(T("Recipe scheduled for execution successfully.").Text);
        }
    }
}