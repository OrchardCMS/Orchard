
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.MetaData {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.Add(T("Site Configuration"), "11",
                        menu => menu
                                    .Add(T("Content Types (metadata)"), "3.1", item => item.Action("ContentTypeList", "Admin", new { area = "Orchard.MetaData" }).Permission(Permissions.ManageMetaData))
                                    );
        }

        
    }
}