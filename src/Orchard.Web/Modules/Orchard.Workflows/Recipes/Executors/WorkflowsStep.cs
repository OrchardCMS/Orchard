using System;
using System.Collections.Generic;
using Orchard.Data;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Workflows.Models;

namespace Orchard.Workflows.Recipes.Executors {
    public class WorkflowsStep : RecipeExecutionStep {
        private readonly IRepository<WorkflowDefinitionRecord> _workflowDefinitionRepository;
        private readonly IRepository<ActivityRecord> _activityRepository;
        private readonly IRepository<TransitionRecord> _transitionRepository;

        public WorkflowsStep(
            IRepository<WorkflowDefinitionRecord> workflowDefinitionRepository,
            IRepository<ActivityRecord> activityRepository,
            IRepository<TransitionRecord> transitionRepository) {

            _workflowDefinitionRepository = workflowDefinitionRepository;
            _activityRepository = activityRepository;
            _transitionRepository = transitionRepository;
        }

        public override string Name {
            get { return "Workflows"; }
        }

        public override void Execute(RecipeExecutionContext context) {
            foreach (var workflowDefinitionElement in context.RecipeStep.Step.Elements()) {
                var workflowDefinition = GetOrCreateWorkflowDefinition(workflowDefinitionElement.Attribute("Name").Value);
                var activitiesElement = workflowDefinitionElement.Element("Activities");
                var transitionsElement = workflowDefinitionElement.Element("Transitions");
                var activitiesDictionary = new Dictionary<int, ActivityRecord>();

                workflowDefinition.Enabled = Boolean.Parse(workflowDefinitionElement.Attribute("Enabled").Value);

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
        }

        private WorkflowDefinitionRecord GetOrCreateWorkflowDefinition(string name) {
            var workflowDefinition = _workflowDefinitionRepository.Get(x => x.Name == name);

            if (workflowDefinition == null) {
                workflowDefinition = new WorkflowDefinitionRecord {
                    Name = name
                };
                _workflowDefinitionRepository.Create(workflowDefinition);
            }
            else {
                CleanWorkFlow(workflowDefinition);
            }

            return workflowDefinition;
        }

        private void CleanWorkFlow(WorkflowDefinitionRecord workflowDefinition) {
            foreach (var activityRecord in workflowDefinition.ActivityRecords) {
                _activityRepository.Delete(activityRecord);
            }
            workflowDefinition.ActivityRecords.Clear();

            foreach (var transitionRecord in workflowDefinition.TransitionRecords) {
                _transitionRepository.Delete(transitionRecord);
            }
            workflowDefinition.TransitionRecords.Clear();
        }
    }
}
