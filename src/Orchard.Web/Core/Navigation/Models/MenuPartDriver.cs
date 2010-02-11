using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Orchard.Core.Navigation.Models {
    [UsedImplicitly]
    public class MenuPartDriver : ContentPartDriver<MenuPart> {
        protected override DriverResult Editor(MenuPart part) {
            return ContentPartTemplate(part, "Parts/Navigation.EditMenuPart").Location("primary", "9");
        }

        protected override DriverResult Editor(MenuPart part, IUpdateModel updater) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return ContentPartTemplate(part, "Parts/Navigation.EditMenuPart").Location("primary", "9");
        }
    }
}
