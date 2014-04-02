using Orchard;
using Orchard.UI.Navigation;

namespace Orchard.Azure.MediaServices.Navigation {
    public class MediaMenu : Component, INavigationProvider {
        public string MenuName { get { return "mediaproviders"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("cloudmedia").Add(T("Microsoft Azure Media"), "5", 
                menu => menu.Action("Import", "Media", new { area = "Orchard.Azure.MediaServices" }).Permission(Permissions.ManageCloudMediaContent));
        }
    }
}