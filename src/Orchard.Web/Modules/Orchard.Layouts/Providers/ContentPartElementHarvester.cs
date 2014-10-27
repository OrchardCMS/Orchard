using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Framework.Harvesters;
using Orchard.Layouts.Services;
using Orchard.Layouts.Settings;

namespace Orchard.Layouts.Providers {
    public class ContentPartElementHarvester : Component, IElementHarvester {
        private readonly Work<IContentDefinitionManager> _contentDefinitionManager;
        private readonly Work<ITransactionManager> _transactionManager;
        private readonly Work<ICultureAccessor> _cultureAccessor;
        private readonly Work<IContentPartDisplay> _contentPartDisplay;
        private readonly Work<IElementFactory> _elementFactory;

        public ContentPartElementHarvester(
            Work<IContentDefinitionManager> contentDefinitionManager, 
            Work<ITransactionManager> transactionManager,
            Work<ICultureAccessor> cultureAccessor,
            Work<IContentPartDisplay> contentPartDisplay, 
            Work<IElementFactory> elementFactory) {

            _contentDefinitionManager = contentDefinitionManager;
            _transactionManager = transactionManager;
            _cultureAccessor = cultureAccessor;
            _contentPartDisplay = contentPartDisplay;
            _elementFactory = elementFactory;
        }

        public IEnumerable<ElementDescriptor> HarvestElements(HarvestElementsContext context) {
            var elementType = typeof(Elements.ContentPart);
            var contentPartElement = _elementFactory.Value.Activate(elementType);
            var contentParts = GetContentParts(context);

            return contentParts.Select(contentPart => new ElementDescriptor(elementType, contentPart.Name, T(contentPart.Name), contentPartElement.Category) {
                Displaying = displayContext => Displaying(displayContext)
            });
        }

        private IEnumerable<ContentPartDefinition> GetContentParts(HarvestElementsContext context) {
            var contentTypeDefinition = context.Content != null 
                ? _contentDefinitionManager.Value.GetTypeDefinition(context.Content.ContentItem.ContentType) 
                : default(ContentTypeDefinition);

            var parts = contentTypeDefinition != null
                ? contentTypeDefinition.Parts.Select(x => x.PartDefinition) 
                : _contentDefinitionManager.Value.ListPartDefinitions();

            return parts.Where(p => p.Settings.GetModel<ContentPartLayoutSettings>().Placable);
        }

        private void Displaying(ElementDisplayContext context) {
            var contentItem = context.Content.ContentItem;
            var contentPart = contentItem.Parts.FirstOrDefault(x => x.PartDefinition.Name == context.Element.Descriptor.TypeName);

            if ((contentItem.Id == 0 || context.DisplayType == "Design") && context.Updater != null) {
                // The content item hasn't been stored yet, so bind form values with the content part to represent actual state.
                var controller = (Controller)context.Updater;
                var oldValueProvider = controller.ValueProvider;

                controller.ValueProvider = new DictionaryValueProvider<string>(context.Element.State, _cultureAccessor.Value.CurrentCulture);
                _contentPartDisplay.Value.UpdateEditor(contentPart, context.Updater);
                _transactionManager.Value.Cancel();
                controller.ValueProvider = oldValueProvider;
            }

            var contentPartShape = _contentPartDisplay.Value.BuildDisplay(contentPart, displayType: "Layout");
            context.ElementShape.ContentPart = contentPartShape;
        }
    }
}