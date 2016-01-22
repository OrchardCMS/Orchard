using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.MediaLibrary.Providers {
    public class OEmbedMenu : INavigationProvider {
        public Localizer T { get; set; }

        public OEmbedMenu() {
            T = NullLocalizer.Instance;
        }

        public string MenuName { get { return "mediaproviders"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("oembed")
                .Add(T("Media Url"), "10", 
                    menu => menu.Action("Index", "OEmbed", new { area = "Orchard.MediaLibrary" })
                        .Permission(Permissions.ManageOwnMedia));
        }
    }
}