using System;
using System.Threading.Tasks;
using Orchard.ContentManagement.Handlers;
using Orchard.DisplayManagement.Descriptors;

namespace Orchard.ContentManagement.Drivers {
    public abstract class ContentPartDriver<TContent> : ContentPartDriverBase<TContent> where TContent : ContentPart, new() {
        public override Task<DriverResult> BuildDisplayAsync(BuildDisplayContext context) {
            var part = context.ContentItem.As<TContent>();

            if (part == null) {
                return Task.FromResult<DriverResult>(null);
            }

            var result = Display(part, context.DisplayType, context.New);

            if (result != null) {
                result.ContentPart = part;
            }

            return Task.FromResult<DriverResult>(result);
        }

        public override Task<DriverResult> BuildEditorAsync(BuildEditorContext context) {
            var part = context.ContentItem.As<TContent>();

            if (part == null) {
                return Task.FromResult<DriverResult>(null);
            }

            var result = Editor(part, context.New);

            if (result != null) {
                result.ContentPart = part;
            }

            return Task.FromResult<DriverResult>(result);
        }

        public override Task<DriverResult> UpdateEditorAsync(UpdateEditorContext context) {
            var part = context.ContentItem.As<TContent>();

            if (part == null) {
                return Task.FromResult<DriverResult>(null);
            }

            // checking if the editor needs to be updated (e.g. if it was not hidden)
            var editor = Editor(part, context.New) as ContentShapeResult;

            if (editor != null) {
                ShapeDescriptor descriptor;
                if (context.ShapeTable.Descriptors.TryGetValue(editor.GetShapeType(), out descriptor)) {
                    var placementContext = new ShapePlacementContext {
                        Content = part.ContentItem,
                        ContentType = part.ContentItem.ContentType,
                        Differentiator = editor.GetDifferentiator(),
                        DisplayType = null,
                        Path = context.Path
                    };

                    var location = descriptor.Placement(placementContext).Location;

                    if (String.IsNullOrEmpty(location) || location == "-") {
                        return Task.FromResult<DriverResult>(editor);
                    }

                    var editorGroup = editor.GetGroup() ?? "";
                    var contextGroup = context.GroupId ?? "";

                    if (!String.Equals(editorGroup, contextGroup, StringComparison.OrdinalIgnoreCase)) {
                        return Task.FromResult<DriverResult>(editor);
                    }
                }
            }

            var result = Editor(part, context.Updater, context.New);

            if (result != null) {
                result.ContentPart = part;
            }

            return Task.FromResult<DriverResult>(result);
        }

        protected virtual DriverResult Display(TContent part, string displayType, dynamic shapeHelper) {
            return null;
        }

        protected virtual DriverResult Editor(TContent part, dynamic shapeHelper) {
            return null;
        }

        protected virtual DriverResult Editor(TContent part, IUpdateModel updater, dynamic shapeHelper) {
            return null;
        }
    }
}