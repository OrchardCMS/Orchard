using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Contents.Settings;
using Orchard.Environment;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Framework.Harvesters;
using Orchard.Layouts.Services;
using Orchard.Layouts.Settings;
using Orchard.Mvc.Html;
using Orchard.Utility.Extensions;

namespace Orchard.Layouts.Providers {
    public class ContentPartElementHarvester : Component, IElementHarvester {
        private readonly Work<IContentDefinitionManager> _contentDefinitionManager;
        private readonly Work<IElementFactory> _elementFactory;
        private readonly Work<IElementManager> _elementManager;

        public ContentPartElementHarvester(
            Work<IContentDefinitionManager> contentDefinitionManager,
            Work<IElementFactory> elementFactory,
            Work<IElementManager> elementManager) {

            _contentDefinitionManager = contentDefinitionManager;
            _elementFactory = elementFactory;
            _elementManager = elementManager;
        }

        public IEnumerable<ElementDescriptor> HarvestElements(HarvestElementsContext context) {
            var elementType = typeof(Elements.ContentPart);
            var contentPartElement = _elementFactory.Value.Activate(elementType);
            var contentParts = GetContentParts(context);

            return contentParts.Select(contentPart => {

                var partSettings = contentPart.Settings.TryGetModel<ContentPartSettings>();
                var partDescription = partSettings != null ? partSettings.Description : null;
                var description = T.Encode(!String.IsNullOrWhiteSpace(partDescription) ? partDescription : contentPart.Name);
                return new ElementDescriptor(elementType, contentPart.Name, T.Encode(contentPart.Name.CamelFriendly()), description, contentPartElement.Category) {
                    Displaying = displayContext => Displaying(displayContext),
                    ToolboxIcon = "\uf1b2",
                    StateBag = new Dictionary<string, object> {
                        {"ElementTypeName", contentPart.Name}
                    }
                };
            });
        }

        private IEnumerable<ContentPartDefinition> GetContentParts(HarvestElementsContext context) {
            var contentTypeDefinition = context.Content != null
                ? _contentDefinitionManager.Value.GetTypeDefinition(context.Content.ContentItem.ContentType)
                : default(ContentTypeDefinition);

            var parts = contentTypeDefinition != null
                ? contentTypeDefinition.Parts.Select(x => x.PartDefinition)
                : _contentDefinitionManager.Value.ListPartDefinitions();

            return parts.Where(p => p.Settings.GetModel<ContentPartLayoutSettings>().Placeable);
        }

        private void Displaying(ElementDisplayingContext context) {
            var drivers = _elementManager.Value.GetDrivers(context.Element);

            foreach (var driver in drivers) {
                driver.Displaying(context);
            }
        }
    }
}