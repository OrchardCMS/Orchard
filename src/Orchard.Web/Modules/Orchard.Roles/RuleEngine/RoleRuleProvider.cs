using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Roles.Models;
using Orchard.Security;
using System;
using System.Linq;

namespace Orchard.Roles.RuleEngine {
    public interface IRuleProvider : IEventHandler {
        void Process(dynamic ruleContext);
    }

    public class RoleRuleProvider : IRuleProvider {
        private readonly IAuthenticationService _authenticationService;

        public RoleRuleProvider(IAuthenticationService authenticationService) {
            _authenticationService = authenticationService;
        }

        public void Process(dynamic ruleContext) {
            if (!String.Equals(ruleContext.FunctionName, "role", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            var user = _authenticationService.GetAuthenticatedUser();
            if (user == null) {
                ruleContext.Result = false;
                return;
            }

            var roles = ((object[])ruleContext.Arguments).Cast<string>();
            var userRoles = user.As<IUserRoles>();
            ruleContext.Result = userRoles != null ? userRoles.Roles.Intersect(roles).Count() > 0 : false;
        }
    }
}