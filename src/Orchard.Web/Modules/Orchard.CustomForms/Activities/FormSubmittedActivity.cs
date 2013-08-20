using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.CustomForms.Activities {
    public class FormSubmittedActivity : Event {

        public const string EventName = "FormSubmitted";

        public Localizer T { get; set; }

        public override bool CanStartWorkflow {
            get { return true; }
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            try {

                var contentTypesState = activityContext.GetState<string>("ContentTypes");

                // "" means 'any'
                if (String.IsNullOrEmpty(contentTypesState)) {
                    return true;
                }

                string[] contentTypes = contentTypesState.Split(',');

                var content = workflowContext.Content;

                if (content == null) {
                    return false;
                }

                return contentTypes.Any(contentType => content.ContentItem.TypeDefinition.Name == contentType);
            }
            catch {
                return false;
            }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Done") };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            yield return T("Done");
        }

        public override string Form {
            get {
                return "SelectCustomForms";
            }
        }

        public override string Name {
            get { return EventName; }
        }

        public override LocalizedString Category {
            get { return T("Content Items"); }
        }

        public override LocalizedString Description {
            get { return T("A custom form is submitted."); }
        }
    }

}