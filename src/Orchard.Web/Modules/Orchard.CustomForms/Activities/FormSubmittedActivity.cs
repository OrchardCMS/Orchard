using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.CustomForms.Activities
{
    public class FormSubmittedActivity : Event
    {

        public const string EventName = "FormSubmitted";

        public Localizer T { get; set; }

        public override bool CanStartWorkflow => true;

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            try
            {

                var state = activityContext.GetState<string>("CustomForms");

                // "" means 'any'
                if (string.IsNullOrEmpty(state))
                {
                    return true;
                }

                var content = workflowContext.Tokens["CustomForm"] as ContentItem;

                if (content == null)
                {
                    return false;
                }

                var contentManager = content.ContentManager;
                var identities = state.Split(',').Select(x => new ContentIdentity(x));
                var customForms = identities.Select(contentManager.ResolveIdentity);

                return customForms.Any(x => x == content);

            }
            catch
            {
                return false;
            }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return new[] { T("Done") };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            yield return T("Done");
        }

        public override string Form => "SelectCustomForms";

        public override string Name => EventName;

        public override LocalizedString Category => T("Content Items");

        public override LocalizedString Description => T("A custom form is submitted.");
    }

}