using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Security;

namespace  Orchard.Core.Navigation.Security {
    [UsedImplicitly]
    public class AuthorizationEventHandler : IAuthorizationServiceEventHandler {
        private readonly IContentManager _contentManager;

        public AuthorizationEventHandler(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
            if (!context.Granted && context.Permission.Name == Permissions.ManageMenus.Name && context.Content != null) {
                
                var menuAsContentItem = context.Content.As<ContentItem>();
                if (menuAsContentItem == null || menuAsContentItem.Id <= 0) {
                    return;
                }

                context.Adjusted = true;
                context.Permission = DynamicPermissions.CreateMenuPermission(menuAsContentItem, _contentManager);
            }
        }
    }
}