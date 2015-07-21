using System;
using System.Collections.Generic;
using Orchard.DynamicForms.Services.Models;
using Orchard.Localization;
using Orchard.Scripting.CSharp.Services;
using Orchard.Workflows.Models;
using Orchard.Environment.Extensions;

namespace Orchard.DynamicForms.Activities {
    public class DynamicFormValidatingActivity : DynamicFormActivity {
        private readonly ICSharpService _csharpService;
        private readonly IOrchardServices _orchardServices;
        private readonly IWorkContextAccessor _workContextAccessor;

        public DynamicFormValidatingActivity(ICSharpService csharpService, IOrchardServices orchardServices, IWorkContextAccessor workContextAccessor) {
            _csharpService = csharpService;
            _orchardServices = orchardServices;
            _workContextAccessor = workContextAccessor;
        }

        public const string EventName = "DynamicFormValidating";

        public override string Name {
            get { return EventName; }
        }

        public override LocalizedString Description {
            get { return T("A dynamic form is being validated."); }
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            var script = activityContext.GetState<string>("Script");

            if (!String.IsNullOrWhiteSpace(script)) {
                var submission = (FormSubmissionTokenContext) workflowContext.Tokens["FormSubmission"];

                // Start the script with the new token syntax.
                script = "// #{ }" + System.Environment.NewLine + script;

                if (workflowContext.Content != null)
                    _csharpService.SetParameter("ContentItem", (dynamic) workflowContext.Content.ContentItem);

                _csharpService.SetParameter("Services", _orchardServices);
                _csharpService.SetParameter("WorkContext", _workContextAccessor.GetContext());
                _csharpService.SetParameter("Workflow", workflowContext);
                _csharpService.SetFunction("T", (Func<string, string>) (x => T(x).Text));
                _csharpService.SetFunction("AddModelError", (Action<string, LocalizedString>) ((key, message) => submission.ModelState.AddModelError(key, message.Text)));

                _csharpService.Run(script);
            }

            return base.Execute(workflowContext, activityContext);
        }
    }
}