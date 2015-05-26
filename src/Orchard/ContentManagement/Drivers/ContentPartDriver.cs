using System;
using System.Collections.Generic;
using System.Linq;
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

            // checking if the editor needs to be updated (e.g. if any of the shapes were not hidden)
            DriverResult editor = Editor(part, context.New);
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
                DriverResult result = Editor(part, context.Updater, context.New);

                if (result != null) {
                    result.ContentPart = part;
                }
				
				return Task.FromResult<DriverResult>(result);
            }

            return Task.FromResult<DriverResult>(editor);
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