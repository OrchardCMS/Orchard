using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.Data;
using Orchard.Events;
using Orchard.Workflows.Models;

namespace Orchard.Workflows.ImportExport {
    public interface IExportEventHandler : IEventHandler {
        void Exporting(dynamic context);
        void Exported(dynamic context);
    }

    public class WorkflowsExportEventHandler : IExportEventHandler {
        private readonly IRepository<WorkflowDefinitionRecord> _workflowDefinitionRepository;

        public WorkflowsExportEventHandler(IRepository<WorkflowDefinitionRecord> workflowDefinitionRepository) {
            _workflowDefinitionRepository = workflowDefinitionRepository;
        }

        public void Exporting(dynamic context) {
        }

        public void Exported(dynamic context) {

            if (!((IEnumerable<string>)context.ExportOptions.CustomSteps).Contains("Workflows")) {
                return;
            }

            var workflowDefinitions = _workflowDefinitionRepository.Table.ToList();

            if (!workflowDefinitions.Any()) {
                return;
            }

            var root = new XElement("Workflows");
            context.Document.Element("Orchard").Add(root);

            foreach (var workflowDefinition in workflowDefinitions) {
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

