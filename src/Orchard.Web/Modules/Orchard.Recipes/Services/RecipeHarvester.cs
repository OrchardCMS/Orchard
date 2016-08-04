using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
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

        public IEnumerable<Recipe> HarvestRecipes() {
            return _extensionManager.AvailableExtensions().SelectMany(HarvestRecipes);
        }

        public IEnumerable<Recipe> HarvestRecipes(string extensionId) {
            var extension = _extensionManager.GetExtension(extensionId);
            if (extension != null) {
                return HarvestRecipes(extension);
            }

            Logger.Error("Could not discover recipes because module '{0}' was not found.", extensionId);
            return Enumerable.Empty<Recipe>();
        }

        private IEnumerable<Recipe> HarvestRecipes(ExtensionDescriptor extension) {
            var recipes = new List<Recipe>();
            
            var recipeLocation = Path.Combine(extension.Location, extension.Id, "Recipes");
            var recipeFiles = _webSiteFolder.ListFiles(recipeLocation, true);

            recipeFiles.Where(r => r.EndsWith(".recipe.xml", StringComparison.OrdinalIgnoreCase)).ToList().ForEach(r => {
                try {
                    recipes.Add(_recipeParser.ParseRecipe(_webSiteFolder.ReadFile(r)));
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Error while parsing recipe file '{0}'.", r);
                }
            });
            
            return recipes;
        }
    }
}