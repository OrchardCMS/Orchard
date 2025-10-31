using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.MediaLibrary.Providers
{
    public class ClientStorageMenu : INavigationProvider
    {
        public Localizer T { get; set; }

        public ClientStorageMenu()
        {
            T = NullLocalizer.Instance;
        }

        public string MenuName => "mediaproviders";

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.AddImageSet("clientstorage")
                .Add(T("My Computer"), "5",
                    menu => menu.Action("Index", "ClientStorage", new { area = "Orchard.MediaLibrary" })
                        .Permission(Permissions.ManageOwnMedia)
                        .Permission(Permissions.ImportMediaContent));
        }
    }
}