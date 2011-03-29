using System;
using System.Collections.Generic;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement;
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
            return part == null ? null : Display(part, context.DisplayType, context.New);
        }

        DriverResult IContentPartDriver.BuildEditor(BuildEditorContext context) {
            var part = context.ContentItem.As<TContent>();
            return part == null ? null : Editor(part, context.New);
        }

        DriverResult IContentPartDriver.UpdateEditor(UpdateEditorContext context) {
            var part = context.ContentItem.As<TContent>();
            return part == null ? null : Editor(part, context.Updater, context.New);
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

        protected virtual void GetContentItemMetadata(TContent context, ContentItemMetadata metadata) { return; }

        protected virtual DriverResult Display(TContent part, string displayType, dynamic shapeHelper) { return null; }
        protected virtual DriverResult Editor(TContent part, dynamic shapeHelper) { return null; }
        protected virtual DriverResult Editor(TContent part, IUpdateModel updater, dynamic shapeHelper) { return null; }

        protected virtual void Importing(TContent part, ImportContentContext context) { return; }
        protected virtual void Imported(TContent part, ImportContentContext context) { return; }
        protected virtual void Exporting(TContent part, ExportContentContext context) { return; }
        protected virtual void Exported(TContent part, ExportContentContext context) { return; }

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
            return new ContentShapeResult(shapeType, Prefix, ctx => AddAlternates(shapeBuilder(ctx)));
        }

        private static object AddAlternates(dynamic shape) {
            ShapeMetadata metadata = shape.Metadata;
            ContentPart part = shape.ContentPart;
            var id = part != null ? part.ContentItem.Id.ToString() : String.Empty;
            var shapeType = metadata.Type;
            var contentType = part != null ? part.ContentItem.ContentType : String.Empty;

            // [ShapeType]__[Id] e.g. Parts/Common.Metadata-42
            if ( !string.IsNullOrEmpty(id) ) {
                metadata.Alternates.Add(shapeType + "__" + id);
            }

            // [ShapeType]__[ContentType] e.g. Parts/Common.Metadata-BlogPost
            if ( !string.IsNullOrEmpty(contentType) ) {
                metadata.Alternates.Add(shapeType + "__" + contentType);
            }

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