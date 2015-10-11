using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Features;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Modules.Recipes.Executors {
    public class FeatureStep : RecipeExecutionStep {
        private readonly IFeatureManager _featureManager;

        public FeatureStep(
            IFeatureManager featureManager,
            RecipeExecutionLogger logger) : base(logger) {

            _featureManager = featureManager;
        }

        public override string Name {
            get { return "Feature"; }
        }

        // <Feature enable="f1,f2,f3" disable="f4" />
        // Enable/Disable features.
        public override void Execute(RecipeExecutionContext recipeContext) {
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
                    Logger.Warning("Unrecognized attribute '{0}' encountered; skipping", attribute.Name.LocalName);
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

            if (featuresToDisable.Any()) {
                Logger.Information("Disabling features: {0}", String.Join(";", featuresToDisable));
                _featureManager.DisableFeatures(featuresToDisable, true);
            }
            if (featuresToEnable.Any()) {
                Logger.Information("Enabling features: {0}", String.Join(";", featuresToEnable));
                _featureManager.EnableFeatures(featuresToEnable, true);
            }
        }

        private static List<string> ParseFeatures(string csv) {
            return csv.Split(',')
                .Select(value => value.Trim())
                .Where(sanitizedValue => !String.IsNullOrEmpty(sanitizedValue))
                .ToList();
        }
    }
}