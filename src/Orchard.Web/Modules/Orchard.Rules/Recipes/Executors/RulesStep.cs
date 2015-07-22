using System;
using System.Linq;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Rules.Models;
using Orchard.Rules.Services;

namespace Orchard.Rules.Recipes.Executors {
    public class RulesStep : RecipeExecutionStep {
        private readonly IRulesServices _rulesServices;

        public RulesStep(IRulesServices rulesServices) {
            _rulesServices = rulesServices;
        }

        public override string Name {
            get { return "Rules"; }
        }

        public override void Execute(RecipeExecutionContext context) {
            foreach (var rule in context.RecipeStep.Step.Elements()) {

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
        }
    }
}
