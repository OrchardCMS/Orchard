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
        protected virtual string Prefix { get { return typeof(TContent).Name; } }

        public void GetContentItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<TContent>();
            if (part != null)
                GetContentItemMetadata(part, context.Metadata);
        }

        public abstract Task<DriverResult> BuildDisplayAsync(BuildDisplayContext context);

        public abstract Task<DriverResult> BuildEditorAsync(BuildEditorContext context);

        public abstract Task<DriverResult> UpdateEditorAsync(UpdateEditorContext context);

        private static IEnumerable<ContentShapeResult> GetShapeResults(DriverResult driverResult) {
            if (driverResult is CombinedResult) {
                return ((CombinedResult)driverResult).GetResults().Select(result => result as ContentShapeResult);
            }

            return new[] { driverResult as ContentShapeResult };
        }

        public void Importing(ImportContentContext context) {
            var part = context.ContentItem.As<TContent>();
            if (part != null)
                Importing(part, context);
        }

        public void Imported(ImportContentContext context) {
            var part = context.ContentItem.As<TContent>();
            if (part != null)
                Imported(part, context);
        }

        public void Exporting(ExportContentContext context) {
            var part = context.ContentItem.As<TContent>();
            if (part != null)
                Exporting(part, context);
        }

        public void Exported(ExportContentContext context) {
            var part = context.ContentItem.As<TContent>();
            if (part != null)
                Exported(part, context);
        }

        protected virtual void GetContentItemMetadata(TContent context, ContentItemMetadata metadata) {}

        protected virtual void Importing(TContent part, ImportContentContext context) {}
        protected virtual void Imported(TContent part, ImportContentContext context) {}
        protected virtual void Exporting(TContent part, ExportContentContext context) {}
        protected virtual void Exported(TContent part, ExportContentContext context) {}
        /// <summary>
        /// Import a content part's previously exported (through the <see cref="ExportInfoset"/> method) infoset data. Note that you can
        /// only import data this way that isn't also stored in records (since only the infoset will be populated but not the part record).
        /// </summary>
        /// <param name="part">The content part used for import.</param>
        /// <param name="context">The context object of the import operation.</param>
        protected static void ImportInfoset(TContent part, ImportContentContext context) {
            if (!part.Has<InfosetPart>()) {
                return;
            }

            Action<XElement, bool> importInfoset = (element, versioned) => {
                if (element == null) {
                    return;
                }

                foreach (var attribute in element.Attributes()) {
                    part.Store(attribute.Name.ToString(), attribute.Value, versioned);
                }
            };

            importInfoset(context.Data.Element(GetInfosetXmlElementName(part, true)), true);
            importInfoset(context.Data.Element(GetInfosetXmlElementName(part, false)), false);
        }

        /// <summary>
        /// Export a content part's data that is stored in the infoset.
        /// </summary>
        /// <param name="part">The content part used for export.</param>
        /// <param name="context">The context object of the export operation.</param>
        protected static void ExportInfoset(TContent part, ExportContentContext context) {
            var infosetPart = part.As<InfosetPart>();

            if (infosetPart == null) {
                return;
            }

            Action<XElement, bool> exportInfoset = (element, versioned) => {
                if (element == null) {
                    return;
                }

                var elementName = GetInfosetXmlElementName(part, versioned);
                foreach (var attribute in element.Attributes()) {
                    context.Element(elementName).SetAttributeValue(attribute.Name, attribute.Value);
                }
            };

            exportInfoset(infosetPart.VersionInfoset.Element.Element(part.PartDefinition.Name), true);
            exportInfoset(infosetPart.Infoset.Element.Element(part.PartDefinition.Name), false);
        }

        private static string GetInfosetXmlElementName(TContent part, bool versioned) {
            return part.PartDefinition.Name + "-" + (versioned ? "VersionInfoset" : "Infoset");
        }

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

        protected static object CreateShape(BuildShapeContext context, string shapeType) {
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