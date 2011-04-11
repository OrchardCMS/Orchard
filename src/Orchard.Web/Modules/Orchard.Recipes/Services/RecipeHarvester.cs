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
    public class RecipeHarvester : IRecipeHarvester {
        private readonly IExtensionManager _extensionManager;
        private readonly IWebSiteFolder _webSiteFolder;
        private readonly IRecipeParser _recipeParser;

        public RecipeHarvester(
            IExtensionManager extensionManager,
            IWebSiteFolder webSiteFolder,
            IRecipeParser recipeParser) {
            _extensionManager = extensionManager;
            _webSiteFolder = webSiteFolder;
            _recipeParser = recipeParser;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public IEnumerable<Recipe> HarvestRecipes(string extensionId) {
            var recipes = new List<Recipe>();
            var extension = _extensionManager.GetExtension(extensionId);
            if (extension != null) {
                var recipeLocation = Path.Combine(extension.Location, extensionId, "Recipes");
                var recipeFiles = _webSiteFolder.ListFiles(recipeLocation, true);
                recipes.AddRange(
                    from recipeFile in recipeFiles
                    where recipeFile.EndsWith(".recipe.xml", StringComparison.OrdinalIgnoreCase)
                    select _recipeParser.ParseRecipe(_webSiteFolder.ReadFile(recipeFile)));
            }
            else {
                Logger.Error("Could not discover recipes because module '{0}' was not found.", extensionId);
            }

            return recipes;
        }
    }
}