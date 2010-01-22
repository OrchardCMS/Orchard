using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Pages.Models;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Pages.Security {
    [UsedImplicitly]
    public class Authorization : AuthorizationServiceEvents {
        public override void Adjust(CheckAccessContext context) {
            if (context.Granted == false && 
                context.Content.Is<Page>() && 
                HasOwnerVariation(context.Permission) &&
                HasOwnership(context.User, context.Content)) {

                context.Adjusted = true;
                context.Permission = GetOwnerVariation(context.Permission);
            }
        }

        private static bool HasOwnership(IUser user, IContent content) {
            if (user==null || content==null)
                return false;
            
            var common = content.As<ICommonAspect>();
            if (common==null || common.Owner==null)
                return false;

            return user.Id == common.Owner.Id;
        }

        private static bool HasOwnerVariation(Permission permission) {
            return GetOwnerVariation(permission) != null;
        }

        private static Permission GetOwnerVariation(Permission permission) {
            if (permission.Name == Permissions.PublishOthersPages.Name)
                return Permissions.PublishPages;
            if (permission.Name == Permissions.EditOthersPages.Name)
                return Permissions.EditPages;
            if (permission.Name == Permissions.DeleteOthersPages.Name)
                return Permissions.DeletePages;
            return null;
        }

    }
}
