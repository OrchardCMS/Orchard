using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.DynamicForms.Activities;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.Layouts.Helpers;
using Orchard.Tokens;
using Orchard.UI.Notify;
using Orchard.Workflows.Services;

namespace Orchard.DynamicForms.Handlers {
    public class FormSubmissionCoordinator : FormEventHandlerBase {
        private readonly INotifier _notifier;
        private readonly IWorkflowManager _workflowManager;
        private readonly ITokenizer _tokenizer;

        public FormSubmissionCoordinator(INotifier notifier, IWorkflowManager workflowManager, ITokenizer tokenizer) {
            _notifier = notifier;
            _workflowManager = workflowManager;
            _tokenizer = tokenizer;
        }

        public override void Validated(FormValidatedEventContext context) {
            if (!context.ModelState.IsValid)
                return;

            var form = context.Form;
            var formName = form.Name;
            var values = context.Values;
            var formService = context.FormService;
            var formValuesDictionary = values.ToTokenDictionary();
            var formTokenContext = new FormSubmissionTokenContext {
                Form = form,
                ModelState = context.ModelState,
                PostedValues = values
            };
            var tokenData = new Dictionary<string, object>(formValuesDictionary) {
                {"Updater", context.Updater},
                {"FormSubmission", formTokenContext},
                {"Content", context.Content }
            };

            // Store the submission.
            if (form.StoreSubmission == true) {
                formService.CreateSubmission(formName, values);
            }

            // Create content item.
            var contentItem = default(ContentItem);
            if (form.CreateContent == true && !String.IsNullOrWhiteSpace(form.FormBindingContentType)) {
                contentItem = formService.CreateContentItem(form, context.ValueProvider);
                formTokenContext.CreatedContent = contentItem;
            }

            // Notifiy.
            if (!String.IsNullOrWhiteSpace(form.Notification))
                _notifier.Information(T(_tokenizer.Replace(T(form.Notification).Text, tokenData)));

            // Trigger workflow event.
            _workflowManager.TriggerEvent(DynamicFormSubmittedActivity.EventName, contentItem, () => tokenData);
        }
    }
}
