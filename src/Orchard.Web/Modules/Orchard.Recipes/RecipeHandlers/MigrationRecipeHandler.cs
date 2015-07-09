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

            Logger.Information("Executing recipe step '{0}'; ExecutionId={1}", recipeContext.RecipeStep.Name, recipeContext.ExecutionId);

            bool runAll = false;
            var features = new List<string>();
            foreach (var attribute in recipeContext.RecipeStep.Step.Attributes()) {
                if (String.Equals(attribute.Name.LocalName, "features", StringComparison.OrdinalIgnoreCase)) {
                    features = ParseFeatures(attribute.Value);
                    if (features.Contains("*"))
                        runAll = true;
                }
                else {
                    Logger.Warning("Unrecognized attribute '{0}' encountered; skipping.", attribute.Name.LocalName);
                }
            }

            if (runAll) {
                foreach (var feature in _dataMigrationManager.GetFeaturesThatNeedUpdate()) {
                    Logger.Information("Updating feature '{0}'.", feature);
                    try {
                        _dataMigrationManager.Update(feature);
                    }
                    catch (Exception ex) {
                        Logger.Error(ex, "Error while updating feature '{0}'", feature);
                        throw;
                    }
                }
            }
            else {
                Logger.Information("Updating features: {0}", String.Join(";", features));
                try {
                    _dataMigrationManager.Update(features);
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Error while updating features: {0}", String.Join(";", features));
                    throw;
                }
            }

            recipeContext.Executed = true;
            Logger.Information("Finished executing recipe step '{0}'.", recipeContext.RecipeStep.Name);
        }

        private static List<string> ParseFeatures(string csv) {
            return csv.Split(',')
                .Select(value => value.Trim())
                .Where(sanitizedValue => !String.IsNullOrEmpty(sanitizedValue))
                .ToList();
        }
    }
}