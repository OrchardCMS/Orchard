using System;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.RecipeHandlers {
    public class SettingsRecipeHandler : IRecipeHandler {
        public SettingsRecipeHandler() {
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        ILogger Logger { get; set; }

        /*  
         <Settings>
          <SiteSettingsPart PageSize="30" />
          <CommentSettingsPart enableSpamProtection="true" />
         </Settings>
        */
        // Set site and part settings.
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "Settings", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            foreach (var element in recipeContext.RecipeStep.Step.Elements()) {
                // set part settings.
            }

            recipeContext.Executed = true;
        }
    }
}
