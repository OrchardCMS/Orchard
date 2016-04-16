using System;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.ImportExport.Services;
using Orchard.Localization;

namespace Orchard.ImportExport.Workflow {
    public class RedirectActionForm : IFormProvider {
        private readonly IDeploymentService _deploymentService;
        private readonly IContentManager _contentManager;
        protected dynamic New { get; set; }
        public Localizer T { get; set; }

        public RedirectActionForm(IShapeFactory shapeFactory,
            IDeploymentService deploymentService,
            IContentManager contentManager) {
            _deploymentService = deploymentService;
            _contentManager = contentManager;
            New = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form =
                shape =>
                {

                    var f = New.Form(
                        Id: "ActionDeploy",
                        _Targets: New.SelectList(
                            Id: "TargetIds", Name: "TargetIds",
                            Title: T("Deployment target"),
                            Description: T("Select the targets to deploy this content to."),
                            Size: 10,
                            Multiple: true
                            ),
                        _Action: New.FieldSet(
                            Title: T("Deployment action"),
                            _Deploy: New.Radio(
                                Id: "action-deploy",
                                Name: "DeploymentAction",
                                Value: "Deploy",
                                Title: T("Deploy immediately"),
                                Description: T("Content will be deployed immediately to selected targets.")
                            ),
                            _Queue: New.Radio(
                                Id: "action-queue",
                                Name: "DeploymentAction",
                                Value: "Queue",
                                Title: T("Queue for deploy later"),
                                Description: T("Content will be queued for later deployment using a scheduled deployment subscription.")
                            ))
                        );

                    foreach (var target in _deploymentService.GetDeploymentTargetConfigurations()) {
                        var targetName = _contentManager.GetItemMetadata(target).DisplayText;
                        f._Targets.Add(new SelectListItem { Value = target.Id.ToString(), Text = targetName });
                    }

                    return f;
                };

            context.Form("ActionDeploy", form);
        }
    }
}