using System;
using System.Threading.Tasks;
using Orchard.ContentManagement.Handlers;
using Orchard.DisplayManagement.Descriptors;

namespace Orchard.ContentManagement.Drivers {
    public abstract class AsyncContentPartDriver<TContent> : ContentPartDriverBase<TContent>
        where TContent : ContentPart, new() {
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
            var editor = await EditorAsync(part, context.New) as ContentShapeResult;

            if (editor != null) {
                ShapeDescriptor descriptor;
                if (context.ShapeTable.Descriptors.TryGetValue(editor.GetShapeType(), out descriptor)) {
                    var placementContext = new ShapePlacementContext {
                        Content = part.ContentItem,
                        ContentType = part.ContentItem.ContentType,
                        Differentiator = editor.GetDifferentiator(),
                        DisplayType = null,
                        Path = String.Empty
                    };

                    var location = descriptor.Placement(placementContext).Location;

                    if (String.IsNullOrEmpty(location) || location == "-") {
                        return editor;
                    }
                }
            }

            var result = await EditorAsync(part, context.Updater, context.New);

            if (result != null) {
                result.ContentPart = part;
            }

            return result;
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