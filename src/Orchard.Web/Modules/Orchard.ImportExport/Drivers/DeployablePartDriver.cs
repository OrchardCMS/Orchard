using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
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
            var model = new DeployablePartViewModel {
                Part = part,
                Targets = targets.Select(t => CreateTargetSummary(part, t)).ToList()
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

        private DeployablePartTargetSummary CreateTargetSummary(CommonPart part, IContent target) {
            var targetName = Services.ContentManager.GetItemMetadata(target).DisplayText;
            var itemTarget = _deploymentService.GetDeploymentItemTarget(part, target, false);

            var summary = new DeployablePartTargetSummary {
                TargetId = target.Id,
                Target = targetName,
                LastDeploy = itemTarget != null && itemTarget.DeployedUtc.HasValue ? itemTarget.DeployedUtc : null,
                Status = itemTarget != null ? itemTarget.DeploymentStatus : DeploymentStatus.Unknown
            };

            return summary;
        }
    }
}