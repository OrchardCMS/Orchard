using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentPermissions.Models;
using Orchard.Roles.Models;
using Orchard.Security;

namespace Orchard.Projections.Extensions
{
    public static class SecurityFilterExtension
    {
        private static readonly string[] AnonymousRole = { "Anonymous" };
        private static readonly string[] AuthenticatedRole = { "Authenticated" };
        public static IEnumerable<T> FilterContentItems<T>(this IEnumerable<T> list, IUser currentUser)
            where T : ContentItem
        {
            return list.Where(x => x.As<ContentPermissionsPart>() == null ||
                                   HasAccess(currentUser,x.As<ContentPermissionsPart>()
                              ));
            //Assuming owners can always view their own items.
        }

        public static bool HasAccess(IUser user, ContentPermissionsPart part)
        {

            if (user == null && part == null)
                return true;
            if (part == null || !part.Enabled)
            {
                return true;
            }
            if (part.ViewContent.Contains(AnonymousRole[0]))
            {
                return true;
            }
            if (user != null)
            {
                var isOwner = HasOwnership(user, part.ContentItem);
                var userRoles = user.As<IUserRoles>();
                if (userRoles == null)
                {
                    return part.ViewContent.Contains(AuthenticatedRole[0]); 
                }
                var authorizedRoles = part.ViewContent.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).AsEnumerable().ToList();
                if (isOwner) {
                    authorizedRoles.AddRange(part.ViewOwnContent.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                }
                var rolesToExamine = userRoles.Roles;
                return rolesToExamine.Any(x => authorizedRoles.Contains(x, StringComparer.OrdinalIgnoreCase));
            }
            return false;

        }

        private static bool HasOwnership(IUser user, IContent content)
        {
            if (user == null || content == null)
                return false;

            var common = content.As<ICommonPart>();
            if (common == null || common.Owner == null)
                return false;

            return user.Id == common.Owner.Id;
        }
    }
    

}