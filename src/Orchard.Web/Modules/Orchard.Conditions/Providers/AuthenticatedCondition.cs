using System;
using Orchard.Conditions.Services;
using Orchard.Security;

namespace Orchard.Conditions.Providers {
    public class AuthenticatedCondition : IConditionProvider {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticatedCondition(IAuthenticationService authenticationService) {
            _authenticationService = authenticationService;
        }

        public void Evaluate(ConditionEvaluationContext evaluationContext) { 
            if (!String.Equals(evaluationContext.FunctionName, "authenticated", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            if (_authenticationService.GetAuthenticatedUser() != null) {
                evaluationContext.Result = true;
                return;
            }

            evaluationContext.Result = false;
        }
    }
}