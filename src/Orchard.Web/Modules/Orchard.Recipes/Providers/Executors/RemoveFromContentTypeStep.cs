using System;
using System.Xml;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentTypes.Events;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.Providers.Executors {
    public class RemoveFromContentTypeStep : RecipeExecutionStep {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentDefinitionEventHandler _contentDefinitionEventHandlers;

        public RemoveFromContentTypeStep(RecipeExecutionLogger logger, IContentDefinitionManager contentDefinitionManager,
            IContentDefinitionEventHandler contentDefinitonEventHandlers) : base(logger) {
            _contentDefinitionManager = contentDefinitionManager;
            _contentDefinitionEventHandlers = contentDefinitonEventHandlers;
        }

        public override string Name {
            get { return "RemoveFromContentType"; }
        }

        public override LocalizedString DisplayName {
            get { return T("Remove From Content Type"); }
        }

        public override LocalizedString Description {
            get { return T("Removes a list of parts and fields from a content type."); }
        }

        // <RemoveFromContentType>
        //   <Blog>
        //     <Parts>
        //     </Parts>
        //     <Fields>
        //     </Fields>
        //   </Blog>
        // </RemoveFromContentType>
        public override void Execute(RecipeExecutionContext context) {
            foreach (var metadataElementType in context.RecipeStep.Step.Elements()) {
                Logger.Debug("Processing element '{0}'.", metadataElementType.Name.LocalName);
                var typeName = XmlConvert.DecodeName(metadataElementType.Name.LocalName);

                foreach (var metadataElement in metadataElementType.Elements()) {
                    switch (metadataElement.Name.LocalName) {
                        case "Parts":
                            foreach (var element in metadataElement.Elements()) {
                                var partName = XmlConvert.DecodeName(element.Name.LocalName);

                                Logger.Information("Removing content part '{0}' from content type '{1}'.", partName, typeName);
                                try {
                                    _contentDefinitionManager.AlterTypeDefinition(typeName, typeBuilder => typeBuilder.RemovePart(partName));
                                    _contentDefinitionEventHandlers.ContentPartDetached(new ContentPartDetachedContext { ContentTypeName = typeName, ContentPartName = partName });
                                }
                                catch (Exception ex) {
                                    Logger.Error(ex, "Error while removing content part '{0}' from content type'{1}'.", partName, typeName);
                                    throw;
                                }
                            }
                            break;

                        case "Fields":
                            foreach (var element in metadataElement.Elements()) {
                                var fieldName = XmlConvert.DecodeName(element.Name.LocalName);

                                Logger.Information("Removing content field '{0}' from content type '{1}'.", fieldName, typeName);
                                try {
                                    _contentDefinitionManager.AlterPartDefinition(typeName, typeBuilder => typeBuilder.RemoveField(fieldName));
                                    _contentDefinitionEventHandlers.ContentFieldDetached(new ContentFieldDetachedContext { ContentPartName = typeName, ContentFieldName = fieldName });
                                }
                                catch (Exception ex) {
                                    Logger.Error(ex, "Error while removing content field '{0}' from content type'{1}'.", fieldName, typeName);
                                    throw;
                                }
                            }
                            break;

                        default:
                            Logger.Warning("Unrecognized element '{0}' encountered; skipping",
                                metadataElement.Name.LocalName);
                            break;
                    }
                }
            }
        }
    }
}