using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Data.Migration;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.Providers.Executors {
    public class MigrationStep : RecipeExecutionStep {
        private readonly IDataMigrationManager _dataMigrationManager;

        public MigrationStep(
            IDataMigrationManager dataMigrationManager,
            RecipeExecutionLogger logger) : base(logger) {

            _dataMigrationManager = dataMigrationManager;
        }

        public override string Name { get { return "Migration"; } }

        // <Migration features="f1, f2" /> 
        // <Migration features="*" />
        // Run migration for features.
        public override void Execute(RecipeExecutionContext context) {
            var runAll = false;
            var features = new List<string>();
            foreach (var attribute in context.RecipeStep.Step.Attributes()) {
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
        }

        private static List<string> ParseFeatures(string csv) {
            return csv.Split(',')
                .Select(value => value.Trim())
                .Where(sanitizedValue => !String.IsNullOrEmpty(sanitizedValue))
                .ToList();
        }
    }
}