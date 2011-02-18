using System;
using System.Xml;
using Orchard.ContentManagement.MetaData;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.RecipeHandlers {
    public class SettingsRecipeHandler : IRecipeHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentDefinitionReader _contentDefinitionReader;

        public SettingsRecipeHandler(IContentDefinitionManager contentDefinitionManager, IContentDefinitionReader contentDefinitionReader) {
            _contentDefinitionManager = contentDefinitionManager;
            _contentDefinitionReader = contentDefinitionReader;
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
                var partElement = element;
                var partName = XmlConvert.DecodeName(element.Name.LocalName);
                _contentDefinitionManager.AlterPartDefinition(partName, alteration => _contentDefinitionReader.Merge(partElement, alteration));
            }

            recipeContext.Executed = true;
        }
    }
}
