using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.ContentManagement.Drivers {
    public abstract class ContentPartDriverBase<TContent> : IContentPartDriver where TContent : ContentPart, new() {
        protected virtual string Prefix {
            get { return typeof (TContent).Name; }
        }

        void IContentPartDriver.GetContentItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<TContent>();
            if (part != null) {
                GetContentItemMetadata(part, context.Metadata);
            }
        }

        public abstract Task<DriverResult> BuildDisplayAsync(BuildDisplayContext context);

        public abstract Task<DriverResult> BuildEditorAsync(BuildEditorContext context);

        public abstract Task<DriverResult> UpdateEditorAsync(UpdateEditorContext context);

        public void Importing(ImportContentContext context)
        {
            var part = context.ContentItem.As<TContent>();
            if (part != null) {
                Importing(part, context);
            }
        }

        public void Imported(ImportContentContext context)
        {
            var part = context.ContentItem.As<TContent>();
            if (part != null) {
                Imported(part, context);
            }
        }

        public void Exporting(ExportContentContext context)
        {
            var part = context.ContentItem.As<TContent>();
            if (part != null) {
                Exporting(part, context);
            }
        }

        public void Exported(ExportContentContext context) {
            var part = context.ContentItem.As<TContent>();
            if (part != null) {
                Exported(part, context);
            }
        }

        protected virtual void GetContentItemMetadata(TContent context, ContentItemMetadata metadata) {}

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

        protected virtual ContentShapeResult ContentShapeImplementation(string shapeType, Func<BuildShapeContext, object> shapeBuilder) {
            return new ContentShapeResult(shapeType, Prefix, ctx => {
                var shape = shapeBuilder(ctx);

                if (shape == null) {
                    return null;
                }

                return AddAlternates(shape, ctx);
            });
        }

        public AsyncContentShapeResult ContentShapeAsync(string shapeType, Func<Task<dynamic>> factory) {
            return ContentShapeAsyncImplementation(shapeType, ctx => factory());
        }

        public AsyncContentShapeResult ContentShapeAsync(string shapeType, Func<dynamic, Task<dynamic>> factory) {
            return ContentShapeAsyncImplementation(shapeType, ctx => factory(CreateShape(ctx, shapeType)));
        }

        protected virtual AsyncContentShapeResult ContentShapeAsyncImplementation(string shapeType, Func<BuildShapeContext, Task<object>> shapeBuilder)
        {
            return new AsyncContentShapeResult(shapeType, Prefix, async ctx => {
                var shape = await shapeBuilder(ctx);

                if (shape == null) {
                    return null;
                }

                return AddAlternates(shape, ctx);
            });
        }

        protected static dynamic AddAlternates(dynamic shape, BuildShapeContext ctx) {
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

        protected static object CreateShape(BuildShapeContext context, string shapeType)
        {
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