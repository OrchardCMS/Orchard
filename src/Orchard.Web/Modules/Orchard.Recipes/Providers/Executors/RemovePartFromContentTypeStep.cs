using System;
using System.Xml;
using Orchard.ContentTypes.Services;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.Providers.Executors {
    public class RemovePartFromContentTypeStep : RecipeExecutionStep {
        private readonly IContentDefinitionService _contentDefinitionService;

        public RemovePartFromContentTypeStep(RecipeExecutionLogger logger, IContentDefinitionService contentDefinitionService) : base(logger) {
            _contentDefinitionService = contentDefinitionService;
        }

        public override string Name {
            get { return "RemovePartFromContentType"; }
        }

        public override LocalizedString DisplayName {
            get { return T("Remove Part From Content Type"); }
        }

        public override LocalizedString Description {
            get { return T("Removes a list of parts from a content type."); }
        }

        // <RemovePartFromContentType>
        //   <Blog>
        //     <Parts>
        //     </Parts>
        //   </Blog>
        // </RemovePartFromContentType>
        public override void Execute(RecipeExecutionContext context) {
            foreach (var metadataElementType in context.RecipeStep.Step.Elements()) {
                Logger.Debug("Processing element '{0}'.", metadataElementType.Name.LocalName);
                var typeName = XmlConvert.DecodeName(metadataElementType.Name.LocalName);

                foreach (var metadataElement in metadataElementType.Elements()) {
                    switch (metadataElement.Name.LocalName) {
                        case "Parts":
                            foreach (var element in metadataElement.Elements()) {
                                var partName = XmlConvert.DecodeName(element.Name.LocalName);

                                Logger.Information("Removing content part '{0}' from content type '{1}'.", typeName, partName);
                                try {
                                    _contentDefinitionService.RemovePartFromType(partName, typeName);
                                }
                                catch (Exception ex) {
                                    Logger.Error(ex, "Error while removing content part '{0}' from content type'{1}'.", typeName, partName);
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