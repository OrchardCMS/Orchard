using System;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.RecipeHandlers {
    public class ThemeRecipeHandler : IRecipeHandler {
        public ThemeRecipeHandler() {
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        ILogger Logger { get; set; }

        // <Theme src="http://" enabled="true" current="true />
        // <Theme name="theme1" repository="somethemerepo" version="1.1" replace="true" />
        // install themes from url or feed.
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "Theme", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            bool replace, enabled, current;
            string source, name, version, repository;

            foreach (var attribute in recipeContext.RecipeStep.Step.Attributes()) {
                if (String.Equals(attribute.Name.LocalName, "src", StringComparison.OrdinalIgnoreCase)) {
                    source = attribute.Value;
                }
                else if (String.Equals(attribute.Name.LocalName, "replace", StringComparison.OrdinalIgnoreCase)) {
                    replace = attribute.Value == "true";
                }
                else if (String.Equals(attribute.Name.LocalName, "enabled", StringComparison.OrdinalIgnoreCase)) {
                    enabled = attribute.Value == "true";
                }
                else if (String.Equals(attribute.Name.LocalName, "current", StringComparison.OrdinalIgnoreCase)) {
                    current = attribute.Value == "true";
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

            // download and install theme. 

            recipeContext.Executed = true;
        }
    }
}