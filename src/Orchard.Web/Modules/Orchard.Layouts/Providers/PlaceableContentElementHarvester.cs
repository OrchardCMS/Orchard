using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Contents.Settings;
using Orchard.Environment;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Framework.Harvesters;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Settings;
using Orchard.Layouts.ViewModels;
using ContentItem = Orchard.ContentManagement.ContentItem;

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
                    Removing = RemoveContentItem,
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
            var versionOptions = context.DisplayType == "Design" ? VersionOptions.Latest : VersionOptions.Published;
            var contentItem = contentItemId != null
                ? _contentManager.Value.Get(contentItemId.Value, versionOptions)
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
            var elementViewModel = new PlaceableContentItemViewModel {
                ContentItemId = element.ContentItemId
            };

            if (context.Updater != null) {
                context.Updater.TryUpdateModel(elementViewModel, context.Prefix, null, null);
            }

            var contentItemId = elementViewModel.ContentItemId;
            var contentItem = contentItemId != null 
                ? _contentManager.Value.Get(contentItemId.Value, VersionOptions.Latest) 
                : _contentManager.Value.New(contentTypeName);

            dynamic contentEditorShape;

            if (context.Updater != null) {
                if (contentItem.Id == 0) {
                    _contentManager.Value.Create(contentItem, VersionOptions.Draft);
                }
                else {
                    var isDraftable = contentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable;
                    var versionOptions = isDraftable ? VersionOptions.DraftRequired : VersionOptions.Latest;
                    contentItem = _contentManager.Value.Get(contentItem.Id, versionOptions);
                }

                element.ContentItemId = contentItem.Id;

                // If the placed content item has the CommonPart attached, set its Container property to the Content (if any).
                // This helps preventing widget types from appearing as orphans.
                var commonPart = contentItem.As<ICommonPart>();
                if (commonPart != null)
                    commonPart.Container = context.Content;

                contentItem.IsPlaceableContent(true);
                contentEditorShape = _contentManager.Value.UpdateEditor(contentItem, context.Updater);

                _contentManager.Value.Publish(contentItem);
            }
            else {
                contentEditorShape = _contentManager.Value.BuildEditor(contentItem);
            }

            var elementEditorShape = context.ShapeFactory.EditorTemplate(TemplateName: "Elements.PlaceableContentItem", Model: elementViewModel, Prefix: context.Prefix);
            var editorWrapper = context.ShapeFactory.PlacedContentElementEditor(ContentItem: contentItem);
            var stereotype = contentItem.TypeDefinition.Settings.ContainsKey("Stereotype") ? contentItem.TypeDefinition.Settings["Stereotype"] : default(string);

            if(!String.IsNullOrWhiteSpace(stereotype))
                editorWrapper.Metadata.Alternates.Add(String.Format("PlacedContentElementEditor__{0}", stereotype));

            editorWrapper.Metadata.Position = "Properties:0";
            elementEditorShape.Metadata.Position = "Properties:0";
            contentEditorShape.Metadata.Position = "Properties:0";
            context.EditorResult.Add(elementEditorShape);
            context.EditorResult.Add(contentEditorShape);
            context.EditorResult.Add(editorWrapper);
        }

        private void RemoveContentItem(ElementRemovingContext context) {
            var element = (PlaceableContentItem) context.Element;
            var contentItemId = element.ContentItemId;
            var contentItem = contentItemId != null ? _contentManager.Value.Get(contentItemId.Value, VersionOptions.Latest) : default(ContentItem);

            if(contentItem != null)
                _contentManager.Value.Remove(contentItem);
        }

        private IEnumerable<ContentTypeDefinition> GetPlaceableContentTypeDefinitions() {
            // Select all types that have either "Placeable" set ot true or the "Widget" or "Element" stereotype.
            var contentTypeDefinitionsQuery =
                from contentTypeDefinition in _contentManager.Value.GetContentTypeDefinitions()
                let stereotype = contentTypeDefinition.Settings.ContainsKey("Stereotype") ? contentTypeDefinition.Settings["Stereotype"] : default(string)
                where contentTypeDefinition.Settings.GetModel<ContentTypeLayoutSettings>().Placeable || stereotype == "Widget" || stereotype == "Element"
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