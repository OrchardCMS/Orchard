using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Themes.Services;

namespace Orchard.Themes.Recipes.Executors {
    public class CurrentThemeStep : RecipeExecutionStep {
        private readonly ISiteThemeService _siteThemeService;

        public override string Name {
            get { return "CurrentTheme"; }
        }

        public CurrentThemeStep(ISiteThemeService siteThemeService) {
            _siteThemeService = siteThemeService;
        }

        public override void Execute(RecipeExecutionContext context) {
            var themeId = context.RecipeStep.Step.Attribute("id").Value;
            _siteThemeService.SetSiteTheme(themeId);
        }
    }
}