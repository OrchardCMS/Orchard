using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Orchard.Alias
{
    [OrchardFeature("Orchard.Alias.UI")]
    public class AdminMenu : INavigationProvider
    {
        public Localizer T { get; set; }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder
                .Add(T("Aliases"), "4", item => item.Action("Index", "Admin", new { area = "Orchard.Alias" }).Permission(StandardPermissions.SiteOwner));
        }
    }
}