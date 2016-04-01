using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentPermissions.Settings;
using Orchard.Core.Contents.Settings;
using Orchard.DynamicForms.Services;
using Orchard.Layouts.Models;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.DynamicForms.Security {
    public class AuthorizationEventHandler : IAuthorizationServiceEventHandler {

        private readonly IFormService _formService;
        private readonly IContentManager _contentManager;

        public Localizer T { get; set; }

        
        public AuthorizationEventHandler(IContentManager contentManager, IFormService formService) {
            _contentManager = contentManager;
            _formService = formService;            
            T = NullLocalizer.Instance;
        }

        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
            if (!context.Granted &&
                context.Content.Is<LayoutPart>()) {

                if ((context.Permission.Name == Permissions.SubmitAnyFormForModifyData.Name 
                    || context.Permission.Name == Permissions.SubmitAnyFormForDeleteData.Name )
                    && HasOwnership(context.User, context.Content)) {
                    context.Adjusted = true;
                    context.Permission = Permissions.GetOwnerVariation(context.Permission);
                    return;
                }

                var typeDefinition = context.Content.ContentItem.TypeDefinition;
                
                if (typeDefinition.Settings.GetModel<ContentTypeSettings>().Securable ||
                    typeDefinition.Settings.GetModel<ContentPermissionsTypeSettings>().SecurableContentItems) {
                    // replace permission if a content item specific version exists
                    var permission = GetContentTypeVariation(context.Permission, typeDefinition, context.Content, context.FormName, _contentManager);

                    if (permission != null) {
                        context.Adjusted = true;                        
                        context.Permission = DynamicPermissions.CreateDynamicPermission(permission, typeDefinition, context.Content, context.FormName, T, _contentManager);
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
            return Permissions.GetOwnerVariation(permission) != null;
        }        
        
        private static Permission GetContentTypeVariation(Permission permission, ContentTypeDefinition typeDefinition, IContent content, string formName, IContentManager contentManager) {
            return DynamicPermissions.ConvertToDynamicPermission(permission, typeDefinition, content, formName, contentManager);
        }
    }
}