using System;
using System.Collections.Generic;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Workflows.Models;

namespace Orchard.Workflows.ImportExport {
    public class WorkflowsRecipeHandler : IRecipeHandler {
        private readonly IRepository<WorkflowDefinitionRecord> _workflowDefinitionRepository;

        public WorkflowsRecipeHandler(IRepository<WorkflowDefinitionRecord> workflowDefinitionRepository) {
            _workflowDefinitionRepository = workflowDefinitionRepository;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "Workflows", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            foreach (var workflowDefinitionElement in recipeContext.RecipeStep.Step.Elements()) {
                var workflowDefinition = new WorkflowDefinitionRecord {
                    Name = ProbeWorkflowDefinitionName(workflowDefinitionElement.Attribute("Name").Value),
                    Enabled = Boolean.Parse(workflowDefinitionElement.Attribute("Enabled").Value)
                };
                
                _workflowDefinitionRepository.Create(workflowDefinition);

                var activitiesElement = workflowDefinitionElement.Element("Activities");
                var transitionsElement = workflowDefinitionElement.Element("Transitions");
                var activitiesDictionary = new Dictionary<int, ActivityRecord>();

                foreach (var activityElement in activitiesElement.Elements()) {
                    var localId = Int32.Parse(activityElement.Attribute("Id").Value);
                    var activity = new ActivityRecord {
                        Name = activityElement.Attribute("Name").Value,
                        Start = Boolean.Parse(activityElement.Attribute("Start").Value),
                        X = Int32.Parse(activityElement.Attribute("X").Value),
                        Y = Int32.Parse(activityElement.Attribute("Y").Value),
                        State = activityElement.Element("State").Value
                    };

                    activitiesDictionary.Add(localId, activity);
                    workflowDefinition.ActivityRecords.Add(activity);
                }

                foreach (var transitionElement in transitionsElement.Elements()) {
                    var sourceActivityId = Int32.Parse(transitionElement.Attribute("SourceActivityId").Value);
                    var sourceEndpoint = transitionElement.Attribute("SourceEndpoint").Value;
                    var destinationActivityId = Int32.Parse(transitionElement.Attribute("DestinationActivityId").Value);
                    var destinationEndpoint = transitionElement.Attribute("DestinationEndpoint").Value;

                    workflowDefinition.TransitionRecords.Add(new TransitionRecord {
                        SourceActivityRecord = activitiesDictionary[sourceActivityId],
                        SourceEndpoint = sourceEndpoint,
                        DestinationActivityRecord = activitiesDictionary[destinationActivityId],
                        DestinationEndpoint = destinationEndpoint
                    });
                }
            }

            recipeContext.Executed = true;
        }

        private string ProbeWorkflowDefinitionName(string name) {
            var count = 0;
            var newName = name;
            WorkflowDefinitionRecord workflowDefinition;

            do {
                var localName = newName;
                workflowDefinition = _workflowDefinitionRepository.Get(x => x.Name == localName);

                if (workflowDefinition != null) {
                    newName = string.Format("{0}-{1}", name, ++count);
                }

            } while (workflowDefinition != null);

            return newName;
        }
    }
}
