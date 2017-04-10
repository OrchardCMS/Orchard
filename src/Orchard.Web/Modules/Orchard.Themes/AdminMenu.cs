using Orchard.Localization;
using Orchard.UI.Navigation;
using System.Collections.Generic;
using System.Linq;
namespace Orchard.Themes {
    public class AdminMenu : INavigationProvider {
        public AdminMenu( IEnumerable<IThemeSelector> selector)
        {
            _selector = selector;
            T = NullLocalizer.Instance;
        }
        private readonly IEnumerable<IThemeSelector> _selector;
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            var selectorToMenus = _selector.Where(s => s.CanSet).OrderBy(s => s.Name);
            if (selectorToMenus.Any())
            {
                builder.AddImageSet("themes").Add(T("Themes"), "10", newMenu =>
                {
                    newMenu.LinkToFirstChild(false)
                    .AddClass("themes")
                    .Permission(Permissions.ApplyTheme)
                    .Add(T("Set Theme"), "1", s =>
                    {
                        foreach (var selectorToMenu in selectorToMenus)
                        {
                            s.Add(selectorToMenu.DisplayName, "0", installed => installed
                            .Action("Index", "Admin", new { area = "Orchard.Themes", name = selectorToMenu.Name })
                            .Permission(Permissions.ApplyTheme)
                            .LocalNav());
                        }
                    }

                    );

                });
            }
           // builder.AddImageSet("themes")
            //    .Add(T("Themes"), "10", menu => menu.Action("Index", "Admin", new { area = "Orchard.Themes" }).Permission(Permissions.ApplyTheme)
            //        .Add(T("Installed"), "0", item => item.Action("Index", "Admin", new { area = "Orchard.Themes" }).Permission(Permissions.ApplyTheme).LocalNav()));
        }
    }
}