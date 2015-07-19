using System.Xml.Linq;
using Orchard.Localization;
using Orchard.Recipes.Services;
using Orchard.Themes.Services;

namespace Orchard.Themes.Recipes.Builders {
    public class CurrentThemeStep : RecipeBuilderStep {
        private readonly ISiteThemeService _siteThemeService;

        public CurrentThemeStep(ISiteThemeService siteThemeService) {
            _siteThemeService = siteThemeService;
        }

        public override string Name {
            get { return "CurrentTheme"; }
        }

        public override LocalizedString DisplayName {
            get { return T("Current Theme"); }
        }

        public override LocalizedString Description {
            get { return T("Adds information to the recipe that indicates the current theme."); }
        }

        public override void Build(BuildContext context) {
            var currentThemeId = _siteThemeService.GetCurrentThemeName();
            var root = new XElement("CurrentTheme", new XAttribute("id", currentThemeId));
            context.RecipeDocument.Element("Orchard").Add(root);
        }
    }
}