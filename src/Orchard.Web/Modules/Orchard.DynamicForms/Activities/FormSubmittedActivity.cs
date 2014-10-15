using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.DynamicForms.Elements;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.DynamicForms.Activities {
    public class FormSubmittedActivity : Event {

        public const string EventName = "DynamicFormSubmitted";

        public Localizer T { get; set; }

        public override bool CanStartWorkflow {
            get { return true; }
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            var state = activityContext.GetState<string>("DynamicForms");

            // "" means 'any'.
            if (String.IsNullOrEmpty(state)) {
                return true;
            }

            var form = workflowContext.Tokens["DynamicForm"] as Form;

            if (form == null) {
                return false;
            }

            var formNames = state.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            return formNames.Any(x => x == form.Name);
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Done") };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            yield return T("Done");
        }

        public override string Form {
            get {
                return "SelectDynamicForms";
            }
        }

        public override string Name {
            get { return EventName; }
        }

        public override LocalizedString Category {
            get { return T("Forms"); }
        }

        public override LocalizedString Description {
            get { return T("A dynamic form is submitted."); }
        }
    }

}