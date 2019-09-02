using System;
using System.Linq;
using System.Xml;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentTypes.Events;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.Providers.Executors {
    public class RemoveContentDefinitionStep : RecipeExecutionStep {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentDefinitionEventHandler _contentDefinitonEventHandlers;

        public RemoveContentDefinitionStep(
            RecipeExecutionLogger logger, IContentDefinitionManager contentDefinitionManager, IContentDefinitionEventHandler contentDefinitonEventHandlers) : base(logger) {
            _contentDefinitionManager = contentDefinitionManager;
            _contentDefinitonEventHandlers = contentDefinitonEventHandlers;
        }

        public override string Name {
            get { return "RemoveContentDefinition"; }
        }

        public override LocalizedString DisplayName {
            get { return T("Remove Content Definition"); }
        }

        public override LocalizedString Description {
            get { return T("Removes a list of content definitions."); }
        }

        // <RemoveContentDefinition>
        //  <Types>
        //   <Blog creatable = "true" >
        //    <Body format="abodyformat"/>
        //   </Blog>
        //  </Types>
        //  <Parts>
        //  </Parts>
        // </RemoveContentDefinition>
        public override void Execute(RecipeExecutionContext context) {
            foreach (var metadataElement in context.RecipeStep.Step.Elements()) {
                Logger.Debug("Processing element '{0}'.", metadataElement.Name.LocalName);
                switch (metadataElement.Name.LocalName) {
                    case "Types":
                        foreach (var element in metadataElement.Elements()) {
                            var typeName = XmlConvert.DecodeName(element.Name.LocalName);

                            Logger.Information("Removing content type '{0}'.", typeName);
                            try {
                                _contentDefinitionManager.DeleteTypeDefinition(typeName);
                                _contentDefinitonEventHandlers.ContentTypeRemoved(new ContentTypeRemovedContext {ContentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(typeName)});
                            }
                            catch (Exception ex) {
                                Logger.Error(ex, "Error while removing content type '{0}'.", typeName);
                                throw;
                            }
                        }
                        break;

                    case "Parts":
                        foreach (var element in metadataElement.Elements()) {
                            var partElement = element;
                            var partName = XmlConvert.DecodeName(element.Name.LocalName);

                            Logger.Information("Removing content part definition '{0}'.", partName);
                            try {
                                _contentDefinitionManager.DeletePartDefinition(partName);
                                _contentDefinitonEventHandlers.ContentPartRemoved(new ContentPartRemovedContext {ContentPartDefinition = _contentDefinitionManager.GetPartDefinition(partName)});
                            }
                            catch (Exception ex) {
                                Logger.Error(ex, "Error while removing content part definition for '{0}'.", partName);
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
