using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Data.Migration;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.RecipeHandlers {
    public class MigrationRecipeHandler : IRecipeHandler {
        private readonly IDataMigrationManager _dataMigrationManager;

        public MigrationRecipeHandler(IDataMigrationManager dataMigrationManager) {
            _dataMigrationManager = dataMigrationManager;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        // <Migration features="f1, f2" /> 
        // <Migration features="*" />
        // Run migration for features.
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "Migration", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            bool runAll = false;
            var features = new List<string>();
            foreach (var attribute in recipeContext.RecipeStep.Step.Attributes()) {
                if (String.Equals(attribute.Name.LocalName, "features", StringComparison.OrdinalIgnoreCase)) {
                    features = ParseFeatures(attribute.Value);
                    if (features.Contains("*"))
                        runAll = true;
                }
                else {
                    Logger.Error("Unrecognized attribute {0} encountered in step Migration. Skipping.", attribute.Name.LocalName);
                }
            }

            if (runAll) {
                foreach (var feature in _dataMigrationManager.GetFeaturesThatNeedUpdate()) {
                    _dataMigrationManager.Update(feature);
                }
            }
            else {
                _dataMigrationManager.Update(features);
            }

            // run migrations
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