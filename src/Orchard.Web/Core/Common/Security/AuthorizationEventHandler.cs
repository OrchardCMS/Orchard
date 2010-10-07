using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents;
using Orchard.Core.Contents.Settings;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Core.Common.Security
{
    [UsedImplicitly]
    public class AuthorizationEventHandler : IAuthorizationServiceEventHandler
    {
        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
            if ( !context.Granted &&
                context.Content.Is<CommonPart>() ) {

                if (OwnerVariationExists(context.Permission) &&
                    HasOwnership(context.User, context.Content)) {

                    context.Adjusted = true;
                    context.Permission = GetOwnerVariation(context.Permission);
                }

                var typeDefinition = context.Content.ContentItem.TypeDefinition;

                // replace permission if a more specific version exists
                if ( typeDefinition.Settings.GetModel<ContentTypeSettings>().Creatable ) {
                    var permission = context.Permission;

                    if ( context.Permission.Name == Contents.Permissions.PublishOwnContent.Name ) {
                        permission = DynamicPermissions.CreateDynamicPersion(DynamicPermissions.PublishOwnContent, typeDefinition);
                    }
                    else if ( context.Permission.Name == Contents.Permissions.EditOwnContent.Name ) {
                        permission = DynamicPermissions.CreateDynamicPersion(DynamicPermissions.EditOwnContent, typeDefinition);
                    }
                    else if ( context.Permission.Name == Contents.Permissions.DeleteOwnContent.Name ) {
                        permission = DynamicPermissions.CreateDynamicPersion(DynamicPermissions.DeleteOwnContent, typeDefinition);
                    }
                    else if ( context.Permission.Name == Contents.Permissions.PublishContent.Name ) {
                        permission = DynamicPermissions.CreateDynamicPersion(DynamicPermissions.PublishContent, typeDefinition);
                    }
                    else if ( context.Permission.Name == Contents.Permissions.EditContent.Name ) {
                        permission = DynamicPermissions.CreateDynamicPersion(DynamicPermissions.EditContent, typeDefinition);
                    }
                    else if ( context.Permission.Name == Contents.Permissions.DeleteContent.Name ) {
                        permission = DynamicPermissions.CreateDynamicPersion(DynamicPermissions.DeleteContent, typeDefinition);
                    }

                    if ( permission != context.Permission ) {
                        context.Adjusted = true;
                        context.Permission = permission;
                    }
                }
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
            if (permission.Name == Contents.Permissions.PublishContent.Name)
                return Contents.Permissions.PublishOwnContent;
            if (permission.Name == Contents.Permissions.EditContent.Name)
                return Contents.Permissions.EditOwnContent;
            if (permission.Name == Contents.Permissions.DeleteContent.Name)
                return Contents.Permissions.DeleteOwnContent;
            return null;
        }
    }
}