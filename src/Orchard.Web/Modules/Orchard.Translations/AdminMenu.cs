using System.Linq;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.UI.Navigation;

namespace Orchard.Translations {
    public class AdminMenu : INavigationProvider {
        private readonly ICultureManager _cultureManager;

        public AdminMenu(ICultureManager cultureManager) {
            _cultureManager = cultureManager;
        }

        public Localizer T { get; set; }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("blog")
                .Add(T("Blog"), "1.5", BuildMenu);
        }

        private void BuildMenu(NavigationItemBuilder menu) {
            var cultures = _cultureManager.ListCultures();
            var cultureCount = cultures.Count();
            var singleCulture = cultureCount == 1 ? cultures.ElementAt(0) : null;

            if (cultureCount > 0 && singleCulture == null) {
                menu.Add(T("Translations"), "10.0",
                         item => item.Action("Index", "TranslationAdmin", new { area = "Orchard.Translations" }));
            }
            else if (singleCulture != null)
                menu.Add(T("Translations"), "10.0",
                    item => item.Action("Edit", "TranslationAdmin", new { area = "Orchard.Translations", cultureName = singleCulture }));
        }
    }
}