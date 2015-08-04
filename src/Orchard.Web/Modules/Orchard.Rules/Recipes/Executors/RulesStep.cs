using System;
using System.Linq;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Rules.Models;
using Orchard.Rules.Services;

namespace Orchard.Rules.Recipes.Executors {
    public class RulesStep : RecipeExecutionStep {
        private readonly IRulesServices _rulesServices;

        public RulesStep(
            IRulesServices rulesServices,
            RecipeExecutionLogger logger) : base(logger) {

            _rulesServices = rulesServices;
        }

        public override string Name {
            get { return "Rules"; }
        }

        public override void Execute(RecipeExecutionContext context) {
            foreach (var rule in context.RecipeStep.Step.Elements()) {
                var ruleName = rule.Attribute("Name").Value;
                Logger.Information("Importing rule '{0}'.", ruleName);

                try {
                    var ruleRecord = _rulesServices.CreateRule(ruleName);
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
                catch (Exception ex) {
                    Logger.Error(ex, "Error while importing rule '{0}'.", ruleName);
                    throw;
                }
            }
        }
    }
}
