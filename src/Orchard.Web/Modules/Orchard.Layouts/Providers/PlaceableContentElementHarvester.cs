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
using Orchard.Layouts.Settings;
using Orchard.Widgets.Models;

namespace Orchard.Layouts.Providers {
    public class PlaceableContentElementHarvester : Component, IElementHarvester {
        private readonly Work<IContentManager> _contentManager;

        public PlaceableContentElementHarvester(Work<IContentManager> contentManager) {
            _contentManager = contentManager;
        }

        public IEnumerable<ElementDescriptor> HarvestElements(HarvestElementsContext context) {
            var contentTypeDefinitions = GetPlaceableContentTypeDefinitions();

            return contentTypeDefinitions.Select(contentTypeDefinition => {
                var settings = contentTypeDefinition.Settings;
                var description = settings.ContainsKey("Description") ? settings["Description"] : contentTypeDefinition.DisplayName;
                var stereotype = settings.ContainsKey("Stereotype") ? settings["Stereotype"] : default(string);
                var category = GetCategoryFromStereotype(stereotype);
                return new ElementDescriptor(typeof (PlaceableContentItem), contentTypeDefinition.Name, T(contentTypeDefinition.DisplayName), T(description), category) {
                    Displaying = Displaying,
                    Editor = Editor,
                    UpdateEditor = UpdateEditor,
                    ToolboxIcon = "\uf1b2",
                    EnableEditorDialog = true,
                    StateBag = new Dictionary<string, object> {
                        { "ContentTypeName", contentTypeDefinition.Name }
                    }
                };
            });
        }

        private void Displaying(ElementDisplayingContext context) {
            var contentTypeName = (string)context.Element.Descriptor.StateBag["ContentTypeName"];
            var element = (PlaceableContentItem)context.Element;
            var contentItemId = element.ContentItemId;
            var contentItem = contentItemId != null
                ? _contentManager.Value.Get(contentItemId.Value, VersionOptions.Published)
                : _contentManager.Value.New(contentTypeName);

            var contentShape = contentItem != null ? _contentManager.Value.BuildDisplay(contentItem) : default(dynamic);
            context.ElementShape.ContentItem = contentItem;
            context.ElementShape.ContentShape = contentShape;
        }

        private void Editor(ElementEditorContext context) {
            UpdateEditor(context);
        }

        private void UpdateEditor(ElementEditorContext context) {
            var contentTypeName = (string)context.Element.Descriptor.StateBag["ContentTypeName"];
            var element = (PlaceableContentItem) context.Element;
            var contentItemId = element.ContentItemId;
            var contentItem = contentItemId != null 
                ? _contentManager.Value.Get(contentItemId.Value, VersionOptions.Latest) 
                : _contentManager.Value.New(contentTypeName);

            dynamic contentEditorShape;

            if (context.Updater != null) {
                if (contentItem.Id == 0) {
                    _contentManager.Value.Create(contentItem, VersionOptions.Draft);
                    element.ContentItemId = contentItem.Id;
                }
                else {
                    contentItem = _contentManager.Value.Get(contentItem.Id, VersionOptions.DraftRequired);
                }

                contentEditorShape = _contentManager.Value.UpdateEditor(contentItem, context.Updater);
            }
            else {
                contentEditorShape = _contentManager.Value.BuildEditor(contentItem);
            }
            
            contentEditorShape.Metadata.Position = "Properties:0";
            context.EditorResult.Add(contentEditorShape);
        }

        private IEnumerable<ContentTypeDefinition> GetPlaceableContentTypeDefinitions() {
            var contentTypeDefinitionsQuery =
                from contentTypeDefinition in _contentManager.Value.GetContentTypeDefinitions()
                where contentTypeDefinition.Settings.GetModel<ContentTypeLayoutSettings>().Placeable
                select contentTypeDefinition;

            return contentTypeDefinitionsQuery.ToList();
        }

        private string GetCategoryFromStereotype(string stereotype) {
            switch (stereotype) {
                case "Widget":
                    return "Widgets";
                default:
                    return "Content Items";
            }
        }
    }
}