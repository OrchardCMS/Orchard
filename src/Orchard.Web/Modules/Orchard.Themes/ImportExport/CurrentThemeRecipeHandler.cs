using System;
using Orchard.Environment.Extensions;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Themes.Services;

namespace Orchard.Themes.ImportExport {

    [OrchardFeature("Orchard.Themes.ImportExportCurrentTheme")]
    public class CurrentThemeRecipeHandler : IRecipeHandler {
        private readonly ISiteThemeService _siteThemeService;
        
        public CurrentThemeRecipeHandler(ISiteThemeService siteThemeService) {
            _siteThemeService = siteThemeService;
        }

        public void ExecuteRecipeStep(RecipeContext context) {
            if (!String.Equals(context.RecipeStep.Name, "CurrentTheme", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            var themeId = context.RecipeStep.Step.Attribute("id").Value;
            _siteThemeService.SetSiteTheme(themeId);
            context.Executed = true;
        }
    }
}