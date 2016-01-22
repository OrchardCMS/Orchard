using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Orchard.Events;
using Orchard.Rules.Services;

namespace Orchard.Rules.ImportExport {
    public interface IExportEventHandler : IEventHandler {
        void Exporting(dynamic context);
        void Exported(dynamic context);
    }

    public class RulesExportHandler : IExportEventHandler {
        private readonly IRulesServices _rulesServices;

        public RulesExportHandler(IRulesServices rulesServices) {
            _rulesServices = rulesServices;
        }

        public void Exporting(dynamic context) {
        }

        public void Exported(dynamic context) {

            if (!((IEnumerable<string>)context.ExportOptions.CustomSteps).Contains("Rules")) {
                return;
            }

            var allRules = _rulesServices.GetRules().ToList();
            
            if(!allRules.Any()) {
                return;
            }

            var root = new XElement("Rules");
            context.Document.Element("Orchard").Add(root);

            foreach(var rule in allRules) {
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

