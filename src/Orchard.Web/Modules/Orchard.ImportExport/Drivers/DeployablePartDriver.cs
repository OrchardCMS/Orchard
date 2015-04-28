using System;
using System.Linq;
using System.Web.UI.WebControls;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.Settings;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.ImportExport.ViewModels;
using Orchard.Localization;

namespace Orchard.ImportExport.Drivers {
    [OrchardFeature("Orchard.Deployment")]
    public class CommonPartDriver : ContentPartDriver<CommonPart> {
        private readonly IDeploymentService _deploymentService;

        public CommonPartDriver(IDeploymentService deploymentService,
            IOrchardServices services) {
            _deploymentService = deploymentService;
            T = NullLocalizer.Instance;
            Services = services;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        //GET
        protected override DriverResult Editor(CommonPart part, dynamic shapeHelper) {
            var targets = _deploymentService.GetDeploymentTargetConfigurations();
            var id = part.ContentItem.Id;
            // Don't show deployment info for a new item
            if (id == 0) return null;
            var contentManager = part.ContentItem.ContentManager;
            var typeDefinition = part.ContentItem.TypeDefinition;
            var isDraftable =  typeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable;
            var hasPublished = contentManager.Get(id, VersionOptions.Published) != null;
            var itemDeploymentHistory = part.ContentItem.GetDeploymentHistory()
                .GroupBy(entry => entry.TargetId)
                .Select(group => group.OrderByDescending(entry => entry.DeploymentCompletedUtc).FirstOrDefault())
                .ToDictionary(entry => entry.TargetId, entry => entry);

            var model = new DeployablePartViewModel {
                Part = part,
                IsDraftable = isDraftable,
                HasPublishedVersion = hasPublished,
                Targets = targets.Select(t =>
                    CreateTargetSummary(part, t, itemDeploymentHistory.ContainsKey(t.Id) ? itemDeploymentHistory[t.Id] : null))
                    .ToList()
            };

            return ContentShape("Parts_DeployablePart_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Deployment.DeployablePart",
                    Model: model,
                    Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(
            CommonPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = part;
            updater.TryUpdateModel(model, Prefix, null, null);

            return Editor(part, shapeHelper);
        }

        private DeployablePartTargetSummary CreateTargetSummary(CommonPart part, IContent target, ItemDeploymentEntry deploymentEntry) {
            var targetName = Services.ContentManager.GetItemMetadata(target).DisplayText;
            var itemTarget = _deploymentService.GetDeploymentItemTarget(part, target, false);

            var summary = new DeployablePartTargetSummary {
                TargetId = target.Id,
                Target = targetName,
                LastDeploy = deploymentEntry != null
                    ? deploymentEntry.DeploymentCompletedUtc
                    : itemTarget != null && itemTarget.DeployedUtc.HasValue
                        ? itemTarget.DeployedUtc
                        : null,
                Status = deploymentEntry != null
                    ? deploymentEntry.Status
                    : itemTarget != null
                        ? itemTarget.DeploymentStatus
                        : DeploymentStatus.Unknown,
                Description = deploymentEntry != null ? deploymentEntry.Description : ""
            };

            return summary;
        }
    }
}