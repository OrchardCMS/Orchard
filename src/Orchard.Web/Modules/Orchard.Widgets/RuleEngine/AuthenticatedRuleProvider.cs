using System;
using Orchard.Security;
using Orchard.Widgets.Services;

namespace Orchard.Widgets.RuleEngine {
    public class AuthenticatedRuleProvider : IRuleProvider {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticatedRuleProvider(IAuthenticationService authenticationService) {
            _authenticationService = authenticationService;
        }

        public void Process(RuleContext ruleContext) { 
            if (!String.Equals(ruleContext.FunctionName, "authenticated", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            if (_authenticationService.GetAuthenticatedUser() != null) {
                ruleContext.Result = true;
                return;
            }

            ruleContext.Result = false;
        }
    }
}