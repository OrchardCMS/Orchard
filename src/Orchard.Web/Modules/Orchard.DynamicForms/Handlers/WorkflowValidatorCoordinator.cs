using System.Collections.Generic;
using Orchard.DynamicForms.Activities;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.Environment.Extensions;
using Orchard.Layouts.Helpers;
using Orchard.Workflows.Services;

namespace Orchard.DynamicForms.Handlers {
    public class WorkflowValidatorCoordinator : FormEventHandlerBase {
        private readonly IWorkflowManager _workflowManager;
        public WorkflowValidatorCoordinator(IWorkflowManager workflowManager) {
            _workflowManager = workflowManager;
        }

        public override void Validating(FormValidatingEventContext context) {
            var form = context.Form;
            var values = context.Values;
            var formValuesDictionary = values.ToTokenDictionary();

            var formTokenContext = new FormSubmissionTokenContext {
                Form = form,
                ModelState = context.ModelState,
                PostedValues = values
            };
            var tokensData = new Dictionary<string, object>(formValuesDictionary) {
                {"Updater", context.Updater},
                {"FormSubmission", formTokenContext},
            };

            _workflowManager.TriggerEvent(name: DynamicFormValidatingActivity.EventName, target: null, tokensContext: () => tokensData);
        }
    }
}