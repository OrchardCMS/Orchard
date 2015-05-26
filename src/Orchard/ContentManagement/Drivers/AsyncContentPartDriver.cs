using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orchard.ContentManagement.Handlers;
using Orchard.DisplayManagement.Descriptors;

namespace Orchard.ContentManagement.Drivers {
    public abstract class AsyncContentPartDriver<TContent> : ContentPartDriverBase<TContent> where TContent : ContentPart, new() {
        public override async Task<DriverResult> BuildDisplayAsync(BuildDisplayContext context) {
            var part = context.ContentItem.As<TContent>();

            if (part == null) {
                return null;
            }

            var result = await DisplayAsync(part, context.DisplayType, context.New);

            if (result != null) {
                result.ContentPart = part;
            }

            return result;
        }

        public override async Task<DriverResult> BuildEditorAsync(BuildEditorContext context) {
            var part = context.ContentItem.As<TContent>();

            if (part == null) {
                return null;
            }

            var result = await EditorAsync(part, context.New);

            if (result != null) {
                result.ContentPart = part;
            }

            return result;
        }

        public override async Task<DriverResult> UpdateEditorAsync(UpdateEditorContext context) {
            var part = context.ContentItem.As<TContent>();

            if (part == null) {
                return null;
            }

            // checking if the editor needs to be updated (e.g. if it was not hidden)
            var editor = await EditorAsync(part, context.New);
            IEnumerable<ContentShapeResult> contentShapeResults = GetShapeResults(editor);

            if (contentShapeResults.Any(contentShapeResult => {
                if (contentShapeResult == null) return true;

                ShapeDescriptor descriptor;
                if (context.ShapeTable.Descriptors.TryGetValue(contentShapeResult.GetShapeType(), out descriptor)) {
                    var placementContext = new ShapePlacementContext {
                        Content = part.ContentItem,
                        ContentType = part.ContentItem.ContentType,
                        Differentiator = contentShapeResult.GetDifferentiator(),
                        DisplayType = null,
                        Path = context.Path
                    };

                    var placementInfo = descriptor.Placement(placementContext);

                    var location = placementInfo.Location;

                    if (String.IsNullOrEmpty(location) || location == "-") {
                        return false;
                    }

                    var editorGroup = contentShapeResult.GetGroup();
                    if (string.IsNullOrEmpty(editorGroup)) {
                        editorGroup = placementInfo.GetGroup() ?? "";
                    }
                    var contextGroup = context.GroupId ?? "";

                    if (!String.Equals(editorGroup, contextGroup, StringComparison.OrdinalIgnoreCase)) {
                        return false;
                    }
                }

                return true;
            })) {
            var result = await EditorAsync(part, context.Updater, context.New);

            if (result != null) {
                result.ContentPart = part;
            }

                return result;
            }

            return editor;
        }

        protected virtual Task<DriverResult> DisplayAsync(TContent part, string displayType, dynamic shapeHelper) {
            return Task.FromResult<DriverResult>(null);
        }

        protected virtual Task<DriverResult> EditorAsync(TContent part, dynamic shapeHelper) {
            return Task.FromResult<DriverResult>(null);
        }

        protected virtual Task<DriverResult> EditorAsync(TContent part, IUpdateModel updater, dynamic shapeHelper) {
            return Task.FromResult<DriverResult>(null);
        }
    }
}