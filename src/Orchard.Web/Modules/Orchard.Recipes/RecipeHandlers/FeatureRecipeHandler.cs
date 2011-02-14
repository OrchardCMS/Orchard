using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.RecipeHandlers {
    public class FeatureRecipeHandler : IRecipeHandler {
        public FeatureRecipeHandler() {
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        ILogger Logger { get; set; }

        // <Feature enable="f1,f2,f3" disable="f4" />
        // Enable/Disable features.
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "Feature", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            var featuresToEnable = new List<string>();
            var featuresToDisable = new List<string>();
            foreach (var attribute in recipeContext.RecipeStep.Step.Attributes()) {
                if (String.Equals(attribute.Name.LocalName, "disable", StringComparison.OrdinalIgnoreCase)) {
                    featuresToDisable = ParseFeatures(attribute.Value);
                }
                else if (String.Equals(attribute.Name.LocalName, "enable", StringComparison.OrdinalIgnoreCase)) {
                    featuresToEnable = ParseFeatures(attribute.Value);
                }
                else {
                    Logger.Error("Unrecognized attribute {0} encountered in step Feature. Skipping.", attribute.Name.LocalName);
                }
            }

            // if both, tx and disable happens first.
            // force cascading enabling and disabling.
            // run migrations.

            recipeContext.Executed = true;
        }

        private static List<string> ParseFeatures(string csv) {
            return csv.Split(',')
                .Select(value => value.Trim())
                .Where(sanitizedValue => !String.IsNullOrEmpty(sanitizedValue))
                .ToList();
        }
    }
}