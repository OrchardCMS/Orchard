using System.Linq;
using System.Xml.Linq;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Recipes.Services;
using Orchard.Workflows.Models;

namespace Orchard.Workflows.Recipes.Builders {
    public class WorkflowsStep : RecipeBuilderStep {
        private readonly IRepository<WorkflowDefinitionRecord> _workflowDefinitionRepository;

        public WorkflowsStep(IRepository<WorkflowDefinitionRecord> workflowDefinitionRepository) {
            _workflowDefinitionRepository = workflowDefinitionRepository;
        }

        public override string Name {
            get { return "Workflows"; }
        }

        public override LocalizedString DisplayName {
            get { return T("Workflows"); }
        }

        public override LocalizedString Description {
            get { return T("Exports workflow definitions."); }
        }

        public override void Build(BuildContext context) {
            var workflowDefinitions = _workflowDefinitionRepository.Table.ToList();

            if (!workflowDefinitions.Any()) {
                return;
            }

            var root = new XElement("Workflows");
            context.RecipeDocument.Element("Orchard").Add(root);

            foreach (var workflowDefinition in workflowDefinitions.OrderBy(x => x.Name)) {
                root.Add(new XElement("Workflow",
                    new XAttribute("Name", workflowDefinition.Name),
                    new XAttribute("Enabled", workflowDefinition.Enabled),
                    new XElement("Activities", workflowDefinition.ActivityRecords.Select(activity =>
                        new XElement("Activity",
                            new XAttribute("Id", activity.Id),
                            new XAttribute("Name", activity.Name),
                            new XAttribute("Start", activity.Start),
                            new XAttribute("X", activity.X),
                            new XAttribute("Y", activity.Y),
                            new XElement("State", activity.State)))),
                        new XElement("Transitions", workflowDefinition.TransitionRecords.Select(transition =>
                            new XElement("Transition",
                                new XAttribute("SourceActivityId", transition.SourceActivityRecord.Id),
                                new XAttribute("SourceEndpoint", transition.SourceEndpoint ?? ""),
                                new XAttribute("DestinationActivityId", transition.DestinationActivityRecord.Id),
                                new XAttribute("DestinationEndpoint", transition.DestinationEndpoint ?? ""))))));
            }
        }
    }
}

