using System;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Contents.Settings;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Core.Contents
{
    [UsedImplicitly]
    public class AuthorizationEventHandler : IAuthorizationServiceEventHandler {
        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
            if ( context.Granted || context.Content == null )
                return;

            var typeDefinition = context.Content.ContentItem.TypeDefinition;

            // replace permission if more specific version exists
            if ( typeDefinition.Settings.GetModel<ContentTypeSettings>().Creatable ) {
                Permission permission = context.Permission;

                if ( context.Permission.Name == Permissions.PublishContent.Name )
                    permission = DynamicPermissions.CreateDynamicPersion(DynamicPermissions.PublishContent, typeDefinition);
                if ( context.Permission.Name == Permissions.EditContent.Name)
                    permission = DynamicPermissions.CreateDynamicPersion(DynamicPermissions.EditContent, typeDefinition);
                if ( context.Permission.Name == Permissions.DeleteContent.Name)
                    permission = DynamicPermissions.CreateDynamicPersion(DynamicPermissions.DeleteContent, typeDefinition);

                // converts the permission if the owner is someone else
                if ( HasOtherOwner(context.User, context.Content) ) {

                    if ( permission.Name == String.Format(DynamicPermissions.PublishContent.Name, typeDefinition.Name) )
                        permission = DynamicPermissions.CreateDynamicPersion(DynamicPermissions.PublishOthersContent, typeDefinition);
                    if ( permission.Name == String.Format(DynamicPermissions.EditContent.Name, typeDefinition.Name) )
                        permission = DynamicPermissions.CreateDynamicPersion(DynamicPermissions.EditOthersContent, typeDefinition);
                    if ( permission.Name == String.Format(DynamicPermissions.DeleteContent.Name, typeDefinition.Name) )
                        permission = DynamicPermissions.CreateDynamicPersion(DynamicPermissions.DeleteOthersContent, typeDefinition);
                }

                if ( permission != context.Permission ) {
                    context.Adjusted = true;
                    context.Permission = permission;
                }
            }
        }

        private static bool HasOtherOwner(IUser user, IContent content) {
            if ( user == null || content == null )
                return false;

            var common = content.As<ICommonPart>();
            if ( common == null || common.Owner == null )
                return false;

            return user.Id != common.Owner.Id;
        }
    }
}

