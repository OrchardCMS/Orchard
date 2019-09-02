using System;
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
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;
using Orchard.Mvc.Html;

namespace Orchard.Layouts.Providers {
    public class ContentFieldElementHarvester : Component, IElementHarvester {
        private readonly Work<IContentDefinitionManager> _contentDefinitionManager;
        private readonly Work<ITransactionManager> _transactionManager;
        private readonly Work<ICultureAccessor> _cultureAccessor;
        private readonly Work<IContentFieldDisplay> _contentFieldDisplay;
        private readonly Work<IElementFactory> _elementFactory;

        public ContentFieldElementHarvester(
            Work<IContentDefinitionManager> contentDefinitionManager, 
            Work<ITransactionManager> transactionManager,
            Work<ICultureAccessor> cultureAccessor,
            Work<IContentFieldDisplay> contentFieldDisplay, 
            Work<IElementFactory> elementFactory) {

            _contentDefinitionManager = contentDefinitionManager;
            _transactionManager = transactionManager;
            _cultureAccessor = cultureAccessor;
            _contentFieldDisplay = contentFieldDisplay;
            _elementFactory = elementFactory;
        }

        public IEnumerable<ElementDescriptor> HarvestElements(HarvestElementsContext context) {
            var elementType = typeof(Elements.ContentField);
            var contentFieldElement = _elementFactory.Value.Activate(elementType);
            var tuples = GetContentFieldTuples(context);

            foreach (var tuple in tuples) {
                var part = tuple.Item1;
                var field = tuple.Item2;
                var name = String.Format("{0}.{1}", part.Name, field.Name);
                var displayName = field.DisplayName;
                yield return new ElementDescriptor(elementType, name, T.Encode(displayName), T.Encode(field.DisplayName), contentFieldElement.Category) {
                    Displaying = displayContext => Displaying(displayContext),
                    ToolboxIcon = "\uf1b2"
                };
            }
        }

        private IEnumerable<Tuple<ContentPartDefinition, ContentPartFieldDefinition>> GetContentFieldTuples(HarvestElementsContext context) {
            // If there is no content item provided as context, there are no fields made available.
            if (context.Content == null)
                return Enumerable.Empty<Tuple<ContentPartDefinition, ContentPartFieldDefinition>>();

            var contentTypeDefinition = _contentDefinitionManager.Value.GetTypeDefinition(context.Content.ContentItem.ContentType);
            var parts = contentTypeDefinition.Parts.Select(x => x.PartDefinition);
            var fields = parts.SelectMany(part => part.Fields.Select(field => Tuple.Create(part, field)));
    
            // TODO: Each module should be able to tell which fields are supported as droppable elements.
            var blackList = new string[0];

            return fields.Where(t => blackList.All(x => t.Item2.FieldDefinition.Name != x)).ToList();
        }

        private void Displaying(ElementDisplayingContext context) {
            var contentItem = context.Content.ContentItem;
            var typeName = context.Element.Descriptor.TypeName;
            var contentField = contentItem.GetContentField(typeName);

            if ((contentItem.Id == 0 || context.DisplayType == "Design") && context.Updater != null) {
                // The content item hasn't been stored yet, so bind form values with the content field to represent actual Data.
                var controller = (Controller)context.Updater;
                var oldValueProvider = controller.ValueProvider;

                controller.ValueProvider = context.Element.Data.ToValueProvider(_cultureAccessor.Value.CurrentCulture);
                _contentFieldDisplay.Value.UpdateEditor(contentItem, contentField, context.Updater);
                _transactionManager.Value.Cancel();
                controller.ValueProvider = oldValueProvider;
            }

            var contentFieldShape = _contentFieldDisplay.Value.BuildDisplay(contentItem, contentField, displayType: "Layout");
            context.ElementShape.ContentField = contentFieldShape;
        }
    }
}
