using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Environment;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Framework.Harvesters;
using Orchard.Widgets.Models;

namespace Orchard.Layouts.Providers {
    public class WidgetElementHarvester : Component, IElementHarvester {
        private readonly Work<IContentManager> _contentManager;

        public WidgetElementHarvester(Work<IContentManager> contentManager) {
            _contentManager = contentManager;
        }

        public IEnumerable<ElementDescriptor> HarvestElements(HarvestElementsContext context) {
            var widgetTypeDefinitions = GetWidgetContentTypeDefinitions();

            return widgetTypeDefinitions.Select(widgetTypeDefinition => {
                var widgetDescription = widgetTypeDefinition.Settings.ContainsKey("Description") ? widgetTypeDefinition.Settings["Description"] : widgetTypeDefinition.DisplayName;
                return new ElementDescriptor(typeof (Widget), widgetTypeDefinition.Name, T(widgetTypeDefinition.DisplayName), T(widgetDescription), "Widgets") {
                    Displaying = Displaying,
                    Editor = Editor,
                    UpdateEditor = UpdateEditor,
                    ToolboxIcon = "\uf1b2",
                    EnableEditorDialog = true,
                    StateBag = new Dictionary<string, object> {
                        { "ContentTypeName", widgetTypeDefinition.Name }
                    }
                };
            });
        }

        private void Displaying(ElementDisplayingContext context) {
            var contentTypeName = (string)context.Element.Descriptor.StateBag["ContentTypeName"];
            var element = (Widget)context.Element;
            var widgetId = element.WidgetId;
            var widgetPart = widgetId != null
                ? _contentManager.Value.Get<WidgetPart>(widgetId.Value, VersionOptions.Published)
                : _contentManager.Value.New<WidgetPart>(contentTypeName);

            var widgetShape = widgetPart != null ? _contentManager.Value.BuildDisplay(widgetPart) : default(dynamic);
            context.ElementShape.WidgetPart = widgetPart;
            context.ElementShape.WidgetShape = widgetShape;
        }

        private void Editor(ElementEditorContext context) {
            UpdateEditor(context);
        }

        private void UpdateEditor(ElementEditorContext context) {
            var contentTypeName = (string)context.Element.Descriptor.StateBag["ContentTypeName"];
            
            var element = (Widget) context.Element;
            var widgetId = element.WidgetId;
            var widgetPart = widgetId != null 
                ? _contentManager.Value.Get<WidgetPart>(widgetId.Value, VersionOptions.Latest) 
                : _contentManager.Value.New<WidgetPart>(contentTypeName);

            // Set dummy layer, zone and position.
            var hiddenLayer = _contentManager.Value.Query<LayerPart, LayerPartRecord>("Layer").Where(x => x.Name == "Disabled").Slice(1).Single();
            widgetPart.LayerPart = hiddenLayer;
            widgetPart.Zone = "Elements";
            widgetPart.Position = "1";

            dynamic widgetEditorShape;

            if (context.Updater != null) {
                if (widgetPart.Id == 0) {
                    _contentManager.Value.Create(widgetPart, VersionOptions.Draft);
                    element.WidgetId = widgetPart.Id;
                }
                else {
                    widgetPart = _contentManager.Value.Get<WidgetPart>(widgetPart.Id, VersionOptions.DraftRequired);
                }

                widgetEditorShape = _contentManager.Value.UpdateEditor(widgetPart, context.Updater);
            }
            else {
                widgetEditorShape = _contentManager.Value.BuildEditor(widgetPart);
            }
            
            widgetEditorShape.Metadata.Position = "Properties:0";
            context.EditorResult.Add(widgetEditorShape);
        }

        private IEnumerable<ContentTypeDefinition> GetWidgetContentTypeDefinitions() {
            var widgetTypeDefinitionsQuery =
                from contentTypeDefinition in _contentManager.Value.GetContentTypeDefinitions()
                where contentTypeDefinition.Settings.ContainsKey("Stereotype") && contentTypeDefinition.Settings["Stereotype"] == "Widget"
                select contentTypeDefinition;

            return widgetTypeDefinitionsQuery.ToList();
        }
    }
}