using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Orchard.Localization;
using Orchard.Recipes.Services;
using Orchard.Rules.Services;

namespace Orchard.Rules.Recipes.Builders {

    public class RulesStep : RecipeBuilderStep {
        private readonly IRulesServices _rulesServices;

        public RulesStep(IRulesServices rulesServices) {
            _rulesServices = rulesServices;
        }

        public override string Name {
            get { return "Rules"; }
        }

        public override LocalizedString DisplayName {
            get { return T("Rules"); }
        }

        public override LocalizedString Description {
            get { return T("Exports rules."); }
        }

        public override void Build(BuildContext context) {
            var allRules = _rulesServices.GetRules().ToList();

            if (!allRules.Any()) {
                return;
            }

            var root = new XElement("Rules");
            context.RecipeDocument.Element("Orchard").Add(root);

            foreach (var rule in allRules) {
                root.Add(new XElement("Rule",
                    new XAttribute("Name", rule.Name),
                    new XAttribute("Enabled", rule.Enabled.ToString(CultureInfo.InvariantCulture)),
                    new XElement("Actions", rule.Actions.Select(action =>
                        new XElement("Action",
                            new XAttribute("Type", action.Type ?? string.Empty),
                            new XAttribute("Category", action.Category ?? string.Empty),
                            new XAttribute("Parameters", action.Parameters ?? string.Empty),
                            new XAttribute("Position", action.Position)
                        )
                    )),
                    new XElement("Events", rule.Events.Select(e =>
                        new XElement("Event",
                            new XAttribute("Type", e.Type ?? string.Empty),
                            new XAttribute("Category", e.Category ?? string.Empty),
                            new XAttribute("Parameters", e.Parameters ?? string.Empty)
                        )
                    ))
                ));
            }
        }
    }
}

