using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentPermissions.Models;
using Orchard.Roles.Models;
using Orchard.Security;

namespace Orchard.Projections.Extensions {
    public static class SecurityFilterExtension {
        private static readonly string[] AnonymousRole = { "Anonymous" };
        private static readonly string[] AuthenticatedRole = { "Authenticated" };
        public static IEnumerable<T> FilterContentItems<T>(this IEnumerable<T> list, IUser currentUser)
            where T : ContentItem {
            return list.Where(x => x.As<ContentPermissionsPart>() == null ||
                                   HasAccess(currentUser, x.As<ContentPermissionsPart>()
                              ));
        }

        public static bool HasAccess(IUser user, ContentPermissionsPart part) {

            if (user == null && part == null)
                return true;
            if (part == null || !part.Enabled) {
                return true;
            }
            if (part.ViewContent.Contains(AnonymousRole[0])) {
                return true;
            }
            if (user != null) {
                if (part.ViewContent.Contains(AuthenticatedRole[0])) {
                    return true;
                }

                var userRoles = user.As<IUserRoles>();

                var viewContentRoles = part.ViewContent.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable().ToList();
                if (userRoles.Roles.Any(x => viewContentRoles.Contains(x, StringComparer.OrdinalIgnoreCase))) {
                    return true;
                }

                if (HasOwnership(user, part.ContentItem)) {
                    var viewOwnContentRoles = part.ViewOwnContent.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (viewOwnContentRoles.Contains(AuthenticatedRole[0]) || userRoles.Roles.Any(x => viewOwnContentRoles.Contains(x, StringComparer.OrdinalIgnoreCase))) {
                        return true;
                    }
                }
            }
            return false;

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