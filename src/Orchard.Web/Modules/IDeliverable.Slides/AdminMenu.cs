using Orchard;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace IDeliverable.Slides
{
    public class AdminMenu : Component, INavigationProvider
    {
        public string MenuName => "admin";

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.Add(T("Settings"),
                menu => menu
                    .Add(T("Slides"), "7", slides => slides
                        .Action("Index", "SlideshowProfile", new { area = "IDeliverable.Slides" })
                        .Permission(StandardPermissions.SiteOwner)
                        .Add(T("Profiles"), "1", profiles => profiles
                            .Action("Index", "SlideshowProfile", new { area = "IDeliverable.Slides" })
                            .Permission(StandardPermissions.SiteOwner)
                            .LocalNav())));
        }
    }
}