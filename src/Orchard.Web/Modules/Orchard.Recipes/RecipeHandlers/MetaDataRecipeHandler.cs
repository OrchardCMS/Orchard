using System;
using System.Xml;
using Orchard.ContentManagement.MetaData;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.RecipeHandlers {
    public class MetadataRecipeHandler : IRecipeHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentDefinitionReader _contentDefinitionReader;

        public MetadataRecipeHandler(IContentDefinitionManager contentDefinitionManager, IContentDefinitionReader contentDefinitionReader) {
            _contentDefinitionManager = contentDefinitionManager;
            _contentDefinitionReader = contentDefinitionReader;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

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

            foreach (var metadataElement in recipeContext.RecipeStep.Step.Elements()) {
                switch (metadataElement.Name.LocalName) {
                    case "Types":
                        foreach (var element in metadataElement.Elements()) {
                            var typeElement = element;
                            var typeName = XmlConvert.DecodeName(element.Name.LocalName);
                            _contentDefinitionManager.AlterTypeDefinition(typeName, alteration => _contentDefinitionReader.Merge(typeElement, alteration));
                        }
                        break;
                    case "Parts":
                        // create dynamic part.
                        foreach (var element in metadataElement.Elements()) {
                            var partElement = element;
                            var partName = XmlConvert.DecodeName(element.Name.LocalName);
                            _contentDefinitionManager.AlterPartDefinition(partName, alteration => _contentDefinitionReader.Merge(partElement, alteration));
                        }
                        break;
                    default:
                        Logger.Error("Unrecognized element {0} encountered in step Metadata. Skipping.", metadataElement.Name.LocalName);
                        break;
                }
            }

            recipeContext.Executed = true;
        }
    }
}