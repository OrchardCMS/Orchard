using System;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using IDeliverable.ThemeSettings.Models;
using IDeliverable.ThemeSettings.Services;

namespace IDeliverable.ThemeSettings.ImportExport
{
    [OrchardFeature("IDeliverable.ThemeSettings.ImportExport")]
    public class ThemeSettingsImportHandler : Component, IRecipeHandler
    {
        private readonly IThemeSettingsService _themeSettingsService;
        public ThemeSettingsImportHandler(IThemeSettingsService themeSettingsService)
        {
            _themeSettingsService = themeSettingsService;
        }

        public void ExecuteRecipeStep(RecipeContext recipeContext)
        {
            if (!String.Equals(recipeContext.RecipeStep.Name, "ThemeSettings", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            foreach (var themeElement in recipeContext.RecipeStep.Step.Elements())
            {
                var themeName = themeElement.Attr<string>("Name");

                foreach (var profileElement in themeElement.Elements())
                {
                    var profileName = profileElement.Attr<string>("Name");
                    var profile = _themeSettingsService.GetProfile(profileName) ?? new ThemeProfile();

                    profile.Name = profileElement.Attr<string>("Name");
                    profile.Description = profileElement.Attr<string>("Description");
                    profile.Theme = themeName;
                    profile.IsCurrent = profileElement.Attr<bool>("IsCurrent");
                    profile.Settings = _themeSettingsService.DeserializeSettings(profileElement.Value);

                    _themeSettingsService.SaveProfile(profile);
                }
            }

            recipeContext.Executed = true;
        }
    }
}
