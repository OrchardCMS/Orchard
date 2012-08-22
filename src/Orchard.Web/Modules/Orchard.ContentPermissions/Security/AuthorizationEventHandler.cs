using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Roles.Models;
using Orchard.Security;
using Orchard.ContentPermissions.Models;

namespace Orchard.ContentPermissions.Security {
    public class AuthorizationEventHandler : IAuthorizationServiceEventHandler {
        private readonly IWorkContextAccessor _workContextAccessor;
        private static readonly string[] AnonymousRole = new[] { "Anonymous" };
        private static readonly string[] AuthenticatedRole = new[] { "Authenticated" };

        public AuthorizationEventHandler(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        public void Checking(CheckAccessContext context) { }
        public void Adjust(CheckAccessContext context) { }

        public void Complete(CheckAccessContext context) {

            if (!String.IsNullOrEmpty(_workContextAccessor.GetContext().CurrentSite.SuperUser) 
                && context.User != null
                && String.Equals(context.User.UserName, _workContextAccessor.GetContext().CurrentSite.SuperUser, StringComparison.Ordinal)) {
                context.Granted = true;
                return;
            }

            if (context.Content == null) {
                return;
            }

            var part = context.Content.As<ContentPermissionsPart>();

            // if the content item has no right attached, check on the container
            if (part == null || !part.Enabled) {
                var commonPart = part.As<CommonPart>();
                if(commonPart != null && commonPart.Container != null) {
                    part = commonPart.As<ContentPermissionsPart>();
                }
            }

            if (part == null || !part.Enabled) {
                return;
            }

            var hasOwnership = HasOwnership(context.User, context.Content);

            IEnumerable<string> authorizedRoles;

            if (context.Permission == Core.Contents.Permissions.ViewContent) {
                authorizedRoles = (hasOwnership ? part.ViewOwnContent : part.ViewContent).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (context.Permission == Core.Contents.Permissions.PublishContent) {
                authorizedRoles = (hasOwnership ? part.PublishOwnContent : part.PublishContent).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (context.Permission == Core.Contents.Permissions.EditContent) {
                authorizedRoles = (hasOwnership ? part.EditOwnContent : part.EditContent).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (context.Permission == Core.Contents.Permissions.DeleteContent) {
                authorizedRoles = (hasOwnership ? part.DeleteOwnContent : part.DeleteContent).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else {
                return;
            }

            // determine what set of roles should be examined by the access check
            IEnumerable<string> rolesToExamine;
            if (context.User == null) {
                rolesToExamine = AnonymousRole;
            }
            else if (context.User.Has<IUserRoles>()) {
                // the current user is not null, so get his roles and add "Authenticated" to it
                rolesToExamine = context.User.As<IUserRoles>().Roles;

                // when it is a simulated anonymous user in the admin
                if (!rolesToExamine.Contains(AnonymousRole[0])) {
                    rolesToExamine = rolesToExamine.Concat(AuthenticatedRole);
                }
            }
            else {
                // the user is not null and has no specific role, then it's just "Authenticated"
                rolesToExamine = AuthenticatedRole;
            }
            
            context.Granted = rolesToExamine.Any(x => authorizedRoles.Contains(x, StringComparer.OrdinalIgnoreCase));
            context.Adjusted = true;
        }

        private static bool HasOwnership(IUser user, IContent content) {
            if (user == null || content == null)
                return false;

            var common = content.As<ICommonPart>();
            if (common == null || common.Owner == null)
                return false;

            return user.Id == common.Owner.Id;
        }
    }
}