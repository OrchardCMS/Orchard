using System;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Packaging.Models;
using Orchard.Packaging.Services;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Themes.Services;

namespace Orchard.Recipes.RecipeHandlers {
    public class ThemeRecipeHandler : IRecipeHandler {
        private readonly IPackagingSourceManager _packagingSourceManager;
        private readonly IThemeService _themeService;
        private readonly ISiteThemeService _siteThemeService;

        public ThemeRecipeHandler(IPackagingSourceManager packagingSourceManager, IThemeService themeService, ISiteThemeService siteThemeService) {
            _packagingSourceManager = packagingSourceManager;
            _themeService = themeService;
            _siteThemeService = siteThemeService;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        ILogger Logger { get; set; }

        // <Theme src="http://" enable="true" current="true />
        // <Theme name="theme1" repository="somethemerepo" version="1.1" replace="true" />
        // install themes from url or feed.
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "Theme", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            bool replace, enable, current;
            string source = null, name = null, version = null, repository = null;

            foreach (var attribute in recipeContext.RecipeStep.Step.Attributes()) {
                if (String.Equals(attribute.Name.LocalName, "src", StringComparison.OrdinalIgnoreCase)) {
                    source = attribute.Value;
                }
                else if (String.Equals(attribute.Name.LocalName, "replace", StringComparison.OrdinalIgnoreCase)) {
                    replace = Boolean.Parse(attribute.Value);
                }
                else if (String.Equals(attribute.Name.LocalName, "enable", StringComparison.OrdinalIgnoreCase)) {
                    enable = Boolean.Parse(attribute.Value);
                }
                else if (String.Equals(attribute.Name.LocalName, "current", StringComparison.OrdinalIgnoreCase)) {
                    current = Boolean.Parse(attribute.Value);
                }
                else if (String.Equals(attribute.Name.LocalName, "name", StringComparison.OrdinalIgnoreCase)) {
                    name = attribute.Value;
                }
                else if (String.Equals(attribute.Name.LocalName, "version", StringComparison.OrdinalIgnoreCase)) {
                    version = attribute.Value;
                }
                else if (String.Equals(attribute.Name.LocalName, "repository", StringComparison.OrdinalIgnoreCase)) {
                    repository = attribute.Value;
                }
                else {
                    Logger.Error("Unrecognized attribute {0} encountered in step Theme. Skipping.", attribute.Name.LocalName);
                }
            }

            if (source != null) {
            }
            else {
                if (name == null) {
                    throw new InvalidOperationException("Either name or source is required in a Theme declaration in a recipe file.");
                }
                // download and install theme from the orchard feed or a custom feed if repository is specified.
                bool enforceVersion = version != null;
                bool installed = false;
                PackagingSource packagingSource = null;
                if (repository != null) {
                    enforceVersion = false;
                    packagingSource = new PackagingSource { FeedTitle = repository, FeedUrl = repository };
                }
                foreach (var packagingEntry in _packagingSourceManager.GetExtensionList(packagingSource)) {
                    if (String.Equals(packagingEntry.Title, name, StringComparison.OrdinalIgnoreCase)) {
                        if (enforceVersion && !String.Equals(packagingEntry.Version, version, StringComparison.OrdinalIgnoreCase)) {
                            continue;
                        }
                        // install.
                        installed = true;
                        break;
                    }
                }

                if (!installed) {
                    throw new InvalidOperationException(string.Format("Theme {0} was not found in the specified location.", name));
                }

            }

            recipeContext.Executed = true;
        }
    }
}