using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Security.Permissions;
using System.Linq;
using Orchard.ContentPermissions.Settings;
using Orchard.ContentPermissions.Services;

namespace Orchard.ContentPermissions.Security {
    public class SecurableContentItemsAuthorizationEventHandler : IAuthorizationServiceEventHandler {
        private readonly IContentManager _contentManager;

        public SecurableContentItemsAuthorizationEventHandler(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public Localizer T { get; set; }

        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
            if (!context.Granted &&
                context.Content.Is<ICommonPart>()) {

                if (OwnerVariationExists(context.Permission) &&
                    HasOwnership(context.User, context.Content)) {

                    context.Adjusted = true;
                    context.Permission = GetOwnerVariation(context.Permission);
                }

                var typeDefinition = context.Content.ContentItem.TypeDefinition;

                if (typeDefinition.Settings.GetModel<ContentPermissionsTypeSettings>().SecurableContentItems) {
                    // replace permission if a content item specific version exists
                    var permission = GetContentTypeVariation(context.Permission);

                    if (permission != null) {
                        context.Adjusted = true;
                        context.Permission = DynamicPermissions.CreateItemPermission(permission, context.Content, T, _contentManager);
                    }
                }
            }
        }

        private static bool HasOwnership(IUser user, IContent content) {
            if (user == null || content == null)
                return false;

            var common = content.As<ICommonPart>();
            if (common == null || common.Owner == null)
                return false;

            return user.Id == common.Owner.Id;
        }

        private static bool OwnerVariationExists(Permission permission) {
            return GetOwnerVariation(permission) != null;
        }

        private static Permission GetOwnerVariation(Permission permission) {
            if (permission.Name == Orchard.Core.Contents.Permissions.PublishContent.Name)
                return Orchard.Core.Contents.Permissions.PublishOwnContent;
            if (permission.Name == Orchard.Core.Contents.Permissions.EditContent.Name)
                return Orchard.Core.Contents.Permissions.EditOwnContent;
            if (permission.Name == Orchard.Core.Contents.Permissions.DeleteContent.Name)
                return Orchard.Core.Contents.Permissions.DeleteOwnContent;
            if (permission.Name == Orchard.Core.Contents.Permissions.ViewContent.Name)
                return Orchard.Core.Contents.Permissions.ViewOwnContent;
            if (permission.Name == Orchard.Core.Contents.Permissions.PreviewContent.Name)
                return Orchard.Core.Contents.Permissions.PreviewOwnContent;

            return null;
        }

        private static Permission GetContentTypeVariation(Permission permission) {
            return DynamicPermissions.ConvertToDynamicPermission(permission);
        }

    }
}