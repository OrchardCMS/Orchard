using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.MediaLibrary.Providers.ClientStorage {
    public class NavigationProvider : INavigationProvider {
        public Localizer T { get; set; }

        public NavigationProvider() {
            T = NullLocalizer.Instance;
        }

        public string MenuName { get { return "mediaproviders"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("clientstorage")
                .Add(T("My Computer"), "5", 
                    menu => menu.Action("Index", "ClientStorage", new { area = "Orchard.MediaLibrary" })
                        .Permission(Permissions.ManageMediaContent));
        }
    }
}