using System;
using System.Linq;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Rules.Models;
using Orchard.Rules.Services;

namespace Orchard.Rules.ImportExport {
    public class RulesRecipeHandler : IRecipeHandler {
        private readonly IRulesServices _rulesServices;

        public RulesRecipeHandler(IRulesServices rulesServices) {
            _rulesServices = rulesServices;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        // <Data />
        // Import Data
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "Rules", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            foreach (var rule in recipeContext.RecipeStep.Step.Elements()) {

                var ruleRecord = _rulesServices.CreateRule(rule.Attribute("Name").Value);
                ruleRecord.Enabled = bool.Parse(rule.Attribute("Enabled").Value);
                
                ruleRecord.Actions = rule.Element("Actions").Elements().Select(action => 
                    new ActionRecord {
                        Type = action.Attribute("Type").Value,
                        Category = action.Attribute("Category").Value,
                        Position = int.Parse(action.Attribute("Position").Value),
                        Parameters = action.Attribute("Parameters").Value,
                        RuleRecord = ruleRecord
                    }).ToList();

                ruleRecord.Events = rule.Element("Events").Elements().Select(action => 
                    new EventRecord {
                        Type = action.Attribute("Type").Value,
                        Category = action.Attribute("Category").Value,
                        Parameters = action.Attribute("Parameters").Value,
                        RuleRecord = ruleRecord
                    }).ToList();

            }

            recipeContext.Executed = true;
        }
    }
}
