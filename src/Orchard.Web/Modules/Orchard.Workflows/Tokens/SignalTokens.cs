using System;
using System.Web;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Tokens;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Tokens {
    public class SignalTokens : ITokenProvider {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly Lazy<ISignalService> _signalService;

        public SignalTokens(IWorkContextAccessor workContextAccessor, Lazy<ISignalService> signalService) {
            _workContextAccessor = workContextAccessor;
            _signalService = signalService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Workflow", T("Workflow"), T("Workflow tokens."))
                .Token("TriggerUrl:*", T("TriggerUrl:<signal>"), T("The relative url to call in order to trigger the specified Signal."))
            ;
        }

        public void Evaluate(EvaluateContext context) {
            if (_workContextAccessor.GetContext().HttpContext == null) {
                return;
            }

            context.For<WorkflowContext>("Workflow")
                   .Token(
                       token => token.StartsWith("TriggerUrl:", StringComparison.OrdinalIgnoreCase) ? token.Substring("TriggerUrl:".Length) : null,
                       (token, workflowContext) => {
                           int contentItemId = 0;
                           if (workflowContext.Content != null) {
                               contentItemId = workflowContext.Content.Id;
                           }

                           var url = "~/Workflows/Signal/Trigger?nonce=" + HttpUtility.UrlEncode(_signalService.Value.CreateNonce(contentItemId, token));
                           return new UrlHelper(_workContextAccessor.GetContext().HttpContext.Request.RequestContext).MakeAbsolute(url);
                       });
        }
    }
}