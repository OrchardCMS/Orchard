using Orchard.Conditions.Services;
using Orchard.ContentManagement;
using Orchard.Roles.Models;
using Orchard.Security;
using System;
using System.Linq;

namespace Orchard.Roles.Conditions {
    public class RoleRuleProvider : IConditionProvider {
        private readonly IAuthenticationService _authenticationService;

        public RoleRuleProvider(IAuthenticationService authenticationService) {
            _authenticationService = authenticationService;
        }

        public void Evaluate(ConditionEvaluationContext evaluationContext) {
            if (!String.Equals(evaluationContext.FunctionName, "role", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            var user = _authenticationService.GetAuthenticatedUser();
            if (user == null) {
                evaluationContext.Result = false;
                return;
            }

            var roles = evaluationContext.Arguments.Cast<string>();
            var userRoles = user.As<IUserRoles>();
            evaluationContext.Result = userRoles != null ? userRoles.Roles.Intersect(roles).Count() > 0 : false;
        }
    }
}