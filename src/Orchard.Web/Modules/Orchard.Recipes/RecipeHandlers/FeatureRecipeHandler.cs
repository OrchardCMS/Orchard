using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.RecipeHandlers {
    public class FeatureRecipeHandler : IRecipeHandler {
        private readonly IFeatureManager _featureManager;

        public FeatureRecipeHandler(IFeatureManager featureManager) {
            _featureManager = featureManager;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

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

            var availableFeatures = _featureManager.GetAvailableFeatures().Select(x => x.Id).ToArray();
            foreach (var featureName in featuresToDisable) {
                if (!availableFeatures.Contains(featureName)) {
                    throw new InvalidOperationException(string.Format("Could not disable feature {0} because it was not found.", featureName));
                }
            }

            foreach (var featureName in featuresToEnable) {
                if (!availableFeatures.Contains(featureName)) {
                    throw new InvalidOperationException(string.Format("Could not enable feature {0} because it was not found.", featureName));
                }
            }

            if (featuresToDisable.Count != 0) {
                _featureManager.DisableFeatures(featuresToDisable, true);
            }
            if (featuresToEnable.Count != 0) {
                _featureManager.EnableFeatures(featuresToEnable, true);
            }

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