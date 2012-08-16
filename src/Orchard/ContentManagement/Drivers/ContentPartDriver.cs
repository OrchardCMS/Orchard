using System;
using System.Collections.Generic;
using System.Globalization;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.ContentManagement.Drivers {
    public abstract class ContentPartDriver<TContent> : IContentPartDriver where TContent : ContentPart, new() {
        protected virtual string Prefix { get { return ""; } }

        void IContentPartDriver.GetContentItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<TContent>();
            if (part != null)
                GetContentItemMetadata(part, context.Metadata);
        }

        DriverResult IContentPartDriver.BuildDisplay(BuildDisplayContext context) {
            var part = context.ContentItem.As<TContent>();
            
            if(part == null) {
                return null;
            }

            DriverResult result = Display(part, context.DisplayType, context.New);
            
            if(result != null ) {
                result.ContentPart = part;
            }

            return result;
        }

        DriverResult IContentPartDriver.BuildEditor(BuildEditorContext context) {
            var part = context.ContentItem.As<TContent>();
            
            if (part == null) {
                return null;
            }

            DriverResult result = Editor(part, context.New);
            
            if (result != null) {
                result.ContentPart = part;
            }

            return result;
        }

        DriverResult IContentPartDriver.UpdateEditor(UpdateEditorContext context) {
            var part = context.ContentItem.As<TContent>();
            
            if (part == null) {
                return null;
            }

            // checking if the editor needs to be updated (e.g. if it was not hidden)
            var editor = Editor(part, context.New) as ContentShapeResult;

            if(editor != null) {
                ShapeDescriptor descriptor;
                if(context.ShapeTable.Descriptors.TryGetValue(editor.GetShapeType(), out descriptor)) {
                    var placementContext = new ShapePlacementContext {
                        ContentType = part.ContentItem.ContentType, 
                        Differentiator = editor.GetDifferentiator(),
                        DisplayType = null,
                        Path = String.Empty
                    };

                    var location = descriptor.Placement(placementContext).Location;

                    if(String.IsNullOrEmpty(location) || location == "-") {
                        return editor;
                    }
                }
            }

            DriverResult result = Editor(part, context.Updater, context.New);

            if (result != null) {
                result.ContentPart = part;
            }

            return result;
        }

        void IContentPartDriver.Importing(ImportContentContext context) {
            var part = context.ContentItem.As<TContent>();
            if (part != null)
                Importing(part, context);
        }

        void IContentPartDriver.Imported(ImportContentContext context) {
            var part = context.ContentItem.As<TContent>();
            if (part != null)
                Imported(part, context);
        }

        void IContentPartDriver.Exporting(ExportContentContext context) {
            var part = context.ContentItem.As<TContent>();
            if (part != null)
                Exporting(part, context);
        }

        void IContentPartDriver.Exported(ExportContentContext context) {
            var part = context.ContentItem.As<TContent>();
            if (part != null)
                Exported(part, context);
        }

        protected virtual void GetContentItemMetadata(TContent context, ContentItemMetadata metadata) {}

        protected virtual DriverResult Display(TContent part, string displayType, dynamic shapeHelper) { return null; }
        protected virtual DriverResult Editor(TContent part, dynamic shapeHelper) { return null; }
        protected virtual DriverResult Editor(TContent part, IUpdateModel updater, dynamic shapeHelper) { return null; }

        protected virtual void Importing(TContent part, ImportContentContext context) {}
        protected virtual void Imported(TContent part, ImportContentContext context) {}
        protected virtual void Exporting(TContent part, ExportContentContext context) {}
        protected virtual void Exported(TContent part, ExportContentContext context) {}

        [Obsolete("Provided while transitioning to factory variations")]
        public ContentShapeResult ContentShape(IShape shape) {
            return ContentShapeImplementation(shape.Metadata.Type, ctx => shape).Location("Content");
        }

        public ContentShapeResult ContentShape(string shapeType, Func<dynamic> factory) {
            return ContentShapeImplementation(shapeType, ctx => factory());
        }

        public ContentShapeResult ContentShape(string shapeType, Func<dynamic, dynamic> factory) {
            return ContentShapeImplementation(shapeType, ctx => factory(CreateShape(ctx, shapeType)));
        }

        private ContentShapeResult ContentShapeImplementation(string shapeType, Func<BuildShapeContext, object> shapeBuilder) {
            return new ContentShapeResult(shapeType, Prefix, ctx => {
                var shape = shapeBuilder(ctx);
                                                                 
                if(shape == null) {
                    return null;
                }

                return AddAlternates(shape, ctx);;
            });
        }

        private static dynamic AddAlternates(dynamic shape, BuildShapeContext ctx) {
            ShapeMetadata metadata = shape.Metadata;

            // if no ContentItem property has been set, assign it
            if (shape.ContentItem == null) {
                shape.ContentItem = ctx.ContentItem;
            }

            var shapeType = metadata.Type;

            // [ShapeType]__[Id] e.g. Parts/Common.Metadata-42
            metadata.Alternates.Add(shapeType + "__" + ctx.ContentItem.Id.ToString(CultureInfo.InvariantCulture));
            
            // [ShapeType]__[ContentType] e.g. Parts/Common.Metadata-BlogPost
            metadata.Alternates.Add(shapeType + "__" + ctx.ContentItem.ContentType);
            
            return shape;
        }

        private static object CreateShape(BuildShapeContext context, string shapeType) {
            IShapeFactory shapeFactory = context.New;
            return shapeFactory.Create(shapeType);
        }

        public CombinedResult Combined(params DriverResult[] results) {
            return new CombinedResult(results);
        }

        public IEnumerable<ContentPartInfo> GetPartInfo() {
            var contentPartInfo = new[] {
                new ContentPartInfo {
                    PartName = typeof (TContent).Name,
                    Factory = typePartDefinition => new TContent {TypePartDefinition = typePartDefinition}
                }
            };

            return contentPartInfo;
        }

    }
}