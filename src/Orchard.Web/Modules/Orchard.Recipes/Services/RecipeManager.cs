using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.WebSite;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public class RecipeManager : IRecipeManager {
        private readonly IExtensionManager _extensionManager;
        private readonly IWebSiteFolder _webSiteFolder;
        private readonly IRecipeParser _recipeParser;
        private readonly IEnumerable<IRecipeHandler> _recipeHandlers;

        public RecipeManager(
            IExtensionManager extensionManager, 
            IWebSiteFolder webSiteFolder, 
            IRecipeParser recipeParser, 
            IEnumerable<IRecipeHandler> recipeHandlers) {
            _extensionManager = extensionManager;
            _webSiteFolder = webSiteFolder;
            _recipeParser = recipeParser;
            _recipeHandlers = recipeHandlers;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        ILogger Logger { get; set; }

        public void Execute(Recipe recipe) {
            throw new NotImplementedException();
        }

        public IEnumerable<Recipe> DiscoverRecipes(string extensionName) {
            var recipes = new List<Recipe>();
            var extension = _extensionManager.GetExtension(extensionName);
            if (extension != null) {
                var recipeLocation = Path.Combine(extension.Location, extensionName, "Recipes");
                var recipeFiles = _webSiteFolder.ListFiles(recipeLocation, true);
                recipes.AddRange(
                    from recipeFile in recipeFiles 
                    where recipeFile.EndsWith(".recipe.xml") 
                    select _recipeParser.ParseRecipe(_webSiteFolder.ReadFile(recipeFile)));
            }
            else {
                Logger.Error("Could not discover recipes because module '{0}' was not found.", extensionName);
            }

            return recipes;
        }
    }
}