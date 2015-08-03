using System;
using System.Collections.Generic;
using System.Xml;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentTypes.Events;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.Providers.Executors {
    public class ContentDefinitionStep : RecipeExecutionStep {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentDefinitionReader _contentDefinitionReader;
        private readonly IContentDefinitionEventHandler _contentDefinitonEventHandlers;

        public override string Name { get { return "ContentDefinition"; } }

        public override IEnumerable<string> Names
        {
            get { return new[] {Name, "Metadata"}; }
        }

        public ContentDefinitionStep(
            IContentDefinitionManager contentDefinitionManager, 
            IContentDefinitionReader contentDefinitionReader, 
            IContentDefinitionEventHandler contentDefinitonEventHandlers,
            RecipeExecutionLogger logger) : base(logger) {

            _contentDefinitionManager = contentDefinitionManager;
            _contentDefinitionReader = contentDefinitionReader;
            _contentDefinitonEventHandlers = contentDefinitonEventHandlers;
        }

        /* 
           <ContentDefinition>
            <Types>
             <Blog creatable="true">
              <Body format="abodyformat"/>
             </Blog>
            </Types>
            <Parts>
            </Parts>
           </ContentDefinition>
         */
        // Set type settings and attach parts to types.
        // Create dynamic parts.
        public override void Execute(RecipeExecutionContext context) {
            foreach (var metadataElement in context.RecipeStep.Step.Elements()) {
                Logger.Debug("Processing element '{0}'.", metadataElement.Name.LocalName);
                switch (metadataElement.Name.LocalName) {
                    case "Types":
                        foreach (var element in metadataElement.Elements()) {
                            var typeElement = element;
                            var typeName = XmlConvert.DecodeName(element.Name.LocalName);

                            Logger.Information("Importing content type '{0}'.", typeName);
                            try {
                                _contentDefinitonEventHandlers.ContentTypeImporting(new ContentTypeImportingContext { ContentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(typeName), ContentTypeName = typeName });
                                _contentDefinitionManager.AlterTypeDefinition(typeName, alteration => _contentDefinitionReader.Merge(typeElement, alteration));
                                _contentDefinitonEventHandlers.ContentTypeImported(new ContentTypeImportedContext { ContentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(typeName) });
                            }
                            catch (Exception ex) {
                                Logger.Error(ex, "Error while importing content type '{0}'.", typeName);
                                throw;
                            }
                        }
                        break;

                    case "Parts":
                        foreach (var element in metadataElement.Elements()) {
                            var partElement = element;
                            var partName = XmlConvert.DecodeName(element.Name.LocalName);

                            Logger.Information("Importing content part '{0}'.", partName);
                            try {
                                _contentDefinitonEventHandlers.ContentPartImporting(new ContentPartImportingContext { ContentPartDefinition = _contentDefinitionManager.GetPartDefinition(partName), ContentPartName = partName });
                            _contentDefinitionManager.AlterPartDefinition(partName, alteration => _contentDefinitionReader.Merge(partElement, alteration));
                            _contentDefinitonEventHandlers.ContentPartImported(new ContentPartImportedContext { ContentPartDefinition = _contentDefinitionManager.GetPartDefinition(partName)});
                            }
                            catch (Exception ex) {
                                Logger.Error(ex, "Error while importing content part '{0}'.", partName);
                                throw;
                            }
                        }
                        break;

                    default:
                        Logger.Warning("Unrecognized element '{0}' encountered; skipping", metadataElement.Name.LocalName);
                        break;
                }
            }
        }
    }
}