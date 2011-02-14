using System;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.RecipeHandlers {
    public class MetadataRecipeHandler : IRecipeHandler {
        public MetadataRecipeHandler() {
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        ILogger Logger { get; set; }

        /* 
           <Metadata>
            <Types>
             <Blog creatable="true">
              <Body format="abodyformat"/>
             </Blog>
            </Types>
            <Parts>
            </Parts>
           </Metadata>
         */
        // Set type settings and attach parts to types.
        // Create dynamic parts.
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "Metadata", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            foreach (var element in recipeContext.RecipeStep.Step.Elements()) {
                switch (element.Name.LocalName) {
                    case "Types":
                        // alter type's definition.
                        break;
                    case "Parts":
                        // create dynamic part.
                        break;
                    default:
                        Logger.Error("Unrecognized element {0} encountered in step Metadata. Skipping.", element.Name.LocalName);
                        break;
                }
            }

            // alter definitions.
            recipeContext.Executed = true;
        }
    }
}