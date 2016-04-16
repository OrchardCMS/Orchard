using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.ImportExport.Workflow {
    [OrchardFeature("Orchard.Deployment")]
    public class DeployActivity : Task {
        private readonly IDeploymentService _deploymentService;
        private readonly IContentManager _contentManager;

        public DeployActivity(IContentManager contentManager, IDeploymentService deploymentService) {
            _deploymentService = deploymentService;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            return true;
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return Enumerable.Empty<LocalizedString>();
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            var targetIds = activityContext.GetState<string>("TargetIds");
            if (string.IsNullOrEmpty(targetIds)) {
                yield return T("Content not deployed. No targets selected.");
            }

            var targetIdArray = targetIds.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);

            var deployAction = (DeploymentAction) Enum.Parse(typeof (DeploymentAction), activityContext.GetState<string>("DeploymentAction"));

            foreach (var target in targetIdArray
                .Select(targetId => _contentManager.Get(int.Parse(targetId)))) {
                switch (deployAction) {
                    case DeploymentAction.Deploy:
                        _deploymentService.DeployContentToTarget(workflowContext.Content, target);
                        break;
                    case DeploymentAction.Queue:
                        var targetConfiguration = target;
                        var itemTarget = _deploymentService.GetDeploymentItemTarget(workflowContext.Content, targetConfiguration);
                        itemTarget.DeploymentStatus = DeploymentStatus.Queued;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                yield return T("Content deployed to target.");
            }
        }

        public override string Name {
            get { return "Deploy"; }
        }

        public override LocalizedString Category {
            get { return T("Content"); }
        }

        public override LocalizedString Description {
            get { return T("Deploy content to the specified targets."); }
        }

        public override string Form {
            get { return "ActionDeploy"; }
        }
    }
}