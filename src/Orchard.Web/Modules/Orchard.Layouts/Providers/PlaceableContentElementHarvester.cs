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
using Orchard.Mvc.Html;
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
                return new ElementDescriptor(typeof (PlaceableContentItem), contentTypeDefinition.Name, T.Encode(contentTypeDefinition.DisplayName), T.Encode(description), category: "Content Items") {
                    Displaying = Displaying,
                    Editor = Editor,
                    UpdateEditor = UpdateEditor,
                    ToolboxIcon = "\uf1b2",
                    EnableEditorDialog = true,
                    Removing = RemoveContentItem,
                    Exporting = ExportElement,
                    Importing = ImportElement,
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
            
            elementEditorShape.Metadata.Position = "Properties:0";
            contentEditorShape.Metadata.Position = "Properties:0";
            context.EditorResult.Add(elementEditorShape);
            context.EditorResult.Add(contentEditorShape);
        }

        private void RemoveContentItem(ElementRemovingContext context) {
            var element = (PlaceableContentItem) context.Element;
            var contentItemId = element.ContentItemId;

            // Only remove the content item if no other elements are referencing this one.
            // This can happen if the user cut an element and then pasted it back.
            // That will delete the initial element and create a copy.
            var placeableElements =
                from e in context.Elements.Flatten()
                let p = e as PlaceableContentItem
                where p != null && p.ContentItemId == contentItemId
                select p;

            if (placeableElements.Any())
                return;

            var contentItem = contentItemId != null ? _contentManager.Value.Get(contentItemId.Value, VersionOptions.Latest) : default(ContentItem);

            if(contentItem != null)
                _contentManager.Value.Remove(contentItem);
        }

        private void ExportElement(ExportElementContext context) {
            var element = (PlaceableContentItem)context.Element;
            var contentItemId = element.ContentItemId;
            var contentItem = contentItemId != null ? _contentManager.Value.Get(contentItemId.Value, VersionOptions.Latest) : default(ContentItem);
            var contentItemIdentity = contentItem != null ? _contentManager.Value.GetItemMetadata(contentItem).Identity.ToString() : default(string);

            if (contentItemIdentity != null)
                context.ExportableData["ContentItemId"] = contentItemIdentity;
        }

        private void ImportElement(ImportElementContext context) {
            var contentItemIdentity = context.ExportableData.Get("ContentItemId");

            if (String.IsNullOrWhiteSpace(contentItemIdentity))
                return;

            var contentItem = context.Session.GetItemFromSession(contentItemIdentity);
            var element = (PlaceableContentItem)context.Element;

            element.ContentItemId = contentItem != null ? contentItem.Id : default(int?);
        }

        private IEnumerable<ContentTypeDefinition> GetPlaceableContentTypeDefinitions() {
            // Select all types that have either "Placeable" set to true.
            var contentTypeDefinitionsQuery =
                from contentTypeDefinition in _contentManager.Value.GetContentTypeDefinitions()
                where contentTypeDefinition.Settings.GetModel<ContentTypeLayoutSettings>().Placeable
                select contentTypeDefinition;

            return contentTypeDefinitionsQuery.ToList();
        }
    }
}