using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Roles.Models;
using Orchard.Security;
using Orchard.ContentPermissions.Models;
using Orchard.Security.Permissions;

namespace Orchard.ContentPermissions.Security {
    public class ContentPermissionsPartAuthorizationEventHandler : IAuthorizationServiceEventHandler {
        private readonly IWorkContextAccessor _workContextAccessor;
        private static readonly string[] AnonymousRole = new[] { "Anonymous" };
        private static readonly string[] AuthenticatedRole = new[] { "Authenticated" };

        public ContentPermissionsPartAuthorizationEventHandler(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        public void Checking(CheckAccessContext context) { }
        public void Adjust(CheckAccessContext context) { }

        public void Complete(CheckAccessContext context) {

            if (context.User != null) {
                var superuser = _workContextAccessor.GetContext().CurrentSite.SuperUser;
                if (!string.IsNullOrEmpty(superuser) && string.Equals(context.User.UserName, superuser, StringComparison.Ordinal)) {
                    context.Granted = true;
                    return;
                }
            }

            if (context.Content == null) {
                return;
            }

            var part = context.Content.As<ContentPermissionsPart>();

            // if the content item has no right attached, check on the container
            if (part == null || !part.Enabled) {
                var commonPart = context.Content.As<ICommonPart>();
                if(commonPart != null && commonPart.Container != null) {
                    part = commonPart.Container.As<ContentPermissionsPart>();
                }

                if (part == null || !part.Enabled) {
                    return;
                }
            }

            var hasOwnership = HasOwnership(context.User, context.Content);

            IEnumerable<string> authorizedRoles;

            var grantingPermissions = PermissionNames(context.Permission, Enumerable.Empty<string>()).Distinct().ToArray();

            if (grantingPermissions.Any(grantingPermission => String.Equals(Core.Contents.Permissions.ViewContent.Name, grantingPermission, StringComparison.OrdinalIgnoreCase)))
            {
                authorizedRoles = (hasOwnership ? part.ViewOwnContent : part.ViewContent).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (grantingPermissions.Any(grantingPermission => String.Equals(Core.Contents.Permissions.EditContent.Name, grantingPermission, StringComparison.OrdinalIgnoreCase)))
            {
                authorizedRoles = (hasOwnership ? part.EditOwnContent : part.EditContent).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (grantingPermissions.Any(grantingPermission => String.Equals(Core.Contents.Permissions.PublishContent.Name, grantingPermission, StringComparison.OrdinalIgnoreCase)))
            {
                authorizedRoles = (hasOwnership ? part.PublishOwnContent : part.PublishContent).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (grantingPermissions.Any(grantingPermission => String.Equals(Core.Contents.Permissions.DeleteContent.Name, grantingPermission, StringComparison.OrdinalIgnoreCase)))
            {
                authorizedRoles = (hasOwnership ? part.DeleteOwnContent : part.DeleteContent).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (grantingPermissions.Any(grantingPermission => String.Equals(Core.Contents.Permissions.PreviewContent.Name, grantingPermission, StringComparison.OrdinalIgnoreCase)))
            {
                authorizedRoles = (hasOwnership ? part.PreviewOwnContent : part.PreviewContent).Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
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

        private static IEnumerable<string> PermissionNames(Permission permission, IEnumerable<string> stack)
        {
            // the given name is tested
            yield return permission.Name;

            // iterate implied permissions to grant, it present
            if (permission.ImpliedBy != null && permission.ImpliedBy.Any())
            {
                foreach (var impliedBy in permission.ImpliedBy)
                {
                    // avoid potential recursion
                    if (stack.Contains(impliedBy.Name))
                        continue;

                    // otherwise accumulate the implied permission names recursively
                    foreach (var impliedName in PermissionNames(impliedBy, stack.Concat(new[] { permission.Name })))
                    {
                        yield return impliedName;
                    }
                }
            }

            yield return StandardPermissions.SiteOwner.Name;
        }
    }
}