using System;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Themes.Services;

namespace Orchard.Themes.Recipes.Executors {
    public class CurrentThemeStep : RecipeExecutionStep {
        private readonly ISiteThemeService _siteThemeService;

        public override string Name {
            get { return "CurrentTheme"; }
        }

        public CurrentThemeStep(
            ISiteThemeService siteThemeService,
            RecipeExecutionLogger logger) : base(logger) {
            _siteThemeService = siteThemeService;
        }

        public override void Execute(RecipeExecutionContext context) {
            var themeId = context.RecipeStep.Step.Attribute("id").Value;
            Logger.Information("Setting site theme to '{0}'.", themeId);

            try {
                _siteThemeService.SetSiteTheme(themeId);
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error while setting site theme to '{0}'.", themeId);
                throw;
            }
        }
    }
}