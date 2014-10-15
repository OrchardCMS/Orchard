using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.DynamicForms.Activities;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.Layouts.Helpers;
using Orchard.UI.Notify;
using Orchard.Workflows.Services;

namespace Orchard.DynamicForms.Handlers {
    public class FormSubmissionCoordinator : FormEventHandlerBase {
        private readonly INotifier _notifier;
        private readonly IWorkflowManager _workflowManager;

        public FormSubmissionCoordinator(INotifier notifier, IWorkflowManager workflowManager) {
            _notifier = notifier;
            _workflowManager = workflowManager;
        }

        public override void Validated(FormValidatedEventContext context) {
            if (!context.ModelState.IsValid)
                return;

            var form = context.Form;
            var formName = form.Name;
            var values = context.Values;
            var formService = context.FormService;

            // Store the submission.
            if (form.StoreSubmission == true) {
                formService.CreateSubmission(formName, values);
            }

            // Create content item.
            var contentItem = default(ContentItem);
            if (form.CreateContent == true && !String.IsNullOrWhiteSpace(form.ContentType)) {
                contentItem = formService.CreateContentItem(form, context.ValueProvider);
            }

            // Notifiy.
            if (!String.IsNullOrWhiteSpace(form.Notification))
                _notifier.Information(T(form.Notification));

            // Trigger workflow event.
            var formValuesDictionary = values.ToTokenDictionary();
            _workflowManager.TriggerEvent(FormSubmittedActivity.EventName, contentItem, () => new Dictionary<string, object>(formValuesDictionary) {
                {"DynamicForm", form}
            });
        }
    }
}