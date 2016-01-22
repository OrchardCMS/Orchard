using System;
using System.Xml;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentTypes.Events;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.RecipeHandlers {
    public class MetadataRecipeHandler : IRecipeHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentDefinitionReader _contentDefinitionReader;
        private readonly IContentDefinitionEventHandler _contentDefinitonEventHandlers;

        public MetadataRecipeHandler(
            IContentDefinitionManager contentDefinitionManager, 
            IContentDefinitionReader contentDefinitionReader, 
            IContentDefinitionEventHandler contentDefinitonEventHandlers) {

            _contentDefinitionManager = contentDefinitionManager;
            _contentDefinitionReader = contentDefinitionReader;
            _contentDefinitonEventHandlers = contentDefinitonEventHandlers;
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

                            _contentDefinitonEventHandlers.ContentTypeImporting(new ContentTypeImportingContext { ContentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(typeName), ContentTypeName = typeName });
                            _contentDefinitionManager.AlterTypeDefinition(typeName, alteration => _contentDefinitionReader.Merge(typeElement, alteration));
                            _contentDefinitonEventHandlers.ContentTypeImported(new ContentTypeImportedContext { ContentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(typeName) });
                        }
                        break;
                    case "Parts":
                        // create dynamic part.
                        foreach (var element in metadataElement.Elements()) {
                            var partElement = element;
                            var partName = XmlConvert.DecodeName(element.Name.LocalName);
                            
                            _contentDefinitonEventHandlers.ContentPartImporting(new ContentPartImportingContext { ContentPartDefinition = _contentDefinitionManager.GetPartDefinition(partName), ContentPartName = partName });
                            _contentDefinitionManager.AlterPartDefinition(partName, alteration => _contentDefinitionReader.Merge(partElement, alteration));
                            _contentDefinitonEventHandlers.ContentPartImported(new ContentPartImportedContext { ContentPartDefinition = _contentDefinitionManager.GetPartDefinition(partName)});
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