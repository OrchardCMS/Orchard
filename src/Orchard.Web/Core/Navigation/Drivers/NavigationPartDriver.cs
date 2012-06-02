using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.ViewModels;
using Orchard.Security;

namespace Orchard.Core.Navigation.Drivers {
    
    public class NavigationPartDriver : ContentPartDriver<NavigationPart> {
        private readonly IAuthorizationService _authorizationService;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IContentManager _contentManager;

        public NavigationPartDriver(
            IAuthorizationService authorizationService, 
            IWorkContextAccessor workContextAccessor,
            IContentManager contentManager) {
            _authorizationService = authorizationService;
            _workContextAccessor = workContextAccessor;
            _contentManager = contentManager;
        }

        protected override DriverResult Editor(NavigationPart part, dynamic shapeHelper) {
            var currentUser = _workContextAccessor.GetContext().CurrentUser;
            if (!_authorizationService.TryCheckAccess(Permissions.ManageMainMenu, currentUser, part))
                return null;

            return ContentShape("Parts_Navigation_Edit",
                                () => {
                                    // loads all menu part of type ContentMenuItem linking to the current content item
                                    var model = new NavigationPartViewModel() {
                                        Part = part,
                                        ContentMenuItems = _contentManager.Query<MenuPart>()
                                            .Join<ContentMenuItemPartRecord>().Where(x => x.ContentMenuItemRecord == part.ContentItem.Record).List()
                                    };

                                    return shapeHelper.EditorTemplate(TemplateName: "Parts.Navigation.Edit", Model: model, Prefix: Prefix);
                                });
        }
    }
}