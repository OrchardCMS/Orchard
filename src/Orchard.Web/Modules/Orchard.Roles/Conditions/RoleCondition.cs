using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Roles.Models;
using Orchard.Security;
using System;
using System.Linq;

namespace Orchard.Roles.Conditions {
    public interface IConditionProvider : IEventHandler {
        void Evaluate(dynamic evaluationContext);
    }

    public class RoleConditionProvider : IConditionProvider {
        private readonly IAuthenticationService _authenticationService;

        public RoleConditionProvider(IAuthenticationService authenticationService) {
            _authenticationService = authenticationService;
        }

        public void Evaluate(dynamic evaluationContext) {
            if (!String.Equals(evaluationContext.FunctionName, "role", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            var user = _authenticationService.GetAuthenticatedUser();
            if (user == null) {
                evaluationContext.Result = false;
                return;
            }

            var roles = ((object[])evaluationContext.Arguments).Cast<string>();
            var userRoles = user.As<IUserRoles>();
            evaluationContext.Result = userRoles != null && userRoles.Roles.Intersect(roles).Any();
        }
    }
}