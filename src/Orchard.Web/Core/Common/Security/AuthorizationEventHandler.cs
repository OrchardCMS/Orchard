using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Core.Common.Security
{
    [UsedImplicitly]
    public class AuthorizationEventHandler : IAuthorizationServiceEventHandler
    {
        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context)
        {
            if (!context.Granted &&
                context.Content.Is<CommonPart>() &&
                OwnerVariationExists(context.Permission) &&
                HasOwnership(context.User, context.Content))
            {

                context.Adjusted = true;
                context.Permission = GetOwnerVariation(context.Permission);
            }
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

        private static bool OwnerVariationExists(Permission permission)
        {
            return GetOwnerVariation(permission) != null;
        }

        private static Permission GetOwnerVariation(Permission permission)
        {
            if (permission.Name == Contents.Permissions.PublishOthersContent.Name)
                return Contents.Permissions.PublishContent;
            if (permission.Name == Contents.Permissions.EditOthersContent.Name)
                return Contents.Permissions.EditContent;
            if (permission.Name == Contents.Permissions.DeleteOthersContent.Name)
                return Contents.Permissions.DeleteContent;
            return null;
        }
    }
}