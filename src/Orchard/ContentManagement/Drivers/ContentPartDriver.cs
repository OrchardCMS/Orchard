using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;
using System.Linq;

namespace Orchard.ContentManagement.Drivers {
    public abstract class ContentPartDriver<TContent> : IContentPartDriver where TContent : ContentPart, new() {
        protected virtual string Prefix { get { return typeof(TContent).Name; } }

        void IContentPartDriver.GetContentItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<TContent>();
            if (part != null)
                GetContentItemMetadata(part, context.Metadata);
        }

        DriverResult IContentPartDriver.BuildDisplay(BuildDisplayContext context) {
            var part = context.ContentItem.As<TContent>();

            if (part == null) {
                return null;
            }

            DriverResult result = Display(part, context.DisplayType, context.New);

            if (result != null) {
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

            // Checking if the editor needs to be updated (e.g. if any of the shapes were not hidden).
            DriverResult editor = Editor(part, context.New);
            IEnumerable<ContentShapeResult> contentShapeResults = editor.GetShapeResults();

            if (contentShapeResults.Any(contentShapeResult =>
                contentShapeResult == null || contentShapeResult.WasDisplayed(context))) {
                DriverResult result = Editor(part, context.Updater, context.New);

                if (result != null) {
                    result.ContentPart = part;
                }

                return result;
            }

            return editor;
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

        void IContentPartDriver.ImportCompleted(ImportContentContext context) {
            var part = context.ContentItem.As<TContent>();
            if (part != null)
                ImportCompleted(part, context);
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

        void IContentPartDriver.Cloning(CloneContentContext context) {
            var originalPart = context.ContentItem.As<TContent>();
            var clonePart = context.CloneContentItem.As<TContent>();
            if (originalPart != null && clonePart != null)
                Cloning(originalPart, clonePart, context);
        }

        void IContentPartDriver.Cloned(CloneContentContext context) {
            var originalPart = context.ContentItem.As<TContent>();
            var clonePart = context.CloneContentItem.As<TContent>();
            if (originalPart != null && clonePart != null)
                Cloned(originalPart, clonePart, context);
        }

        protected virtual void GetContentItemMetadata(TContent context, ContentItemMetadata metadata) { }

        protected virtual DriverResult Display(TContent part, string displayType, dynamic shapeHelper) { return null; }
        protected virtual DriverResult Editor(TContent part, dynamic shapeHelper) { return null; }
        protected virtual DriverResult Editor(TContent part, IUpdateModel updater, dynamic shapeHelper) { return null; }

        protected virtual void Importing(TContent part, ImportContentContext context) { }
        protected virtual void Imported(TContent part, ImportContentContext context) { }
        protected virtual void ImportCompleted(TContent part, ImportContentContext context) { }
        protected virtual void Exporting(TContent part, ExportContentContext context) { }
        protected virtual void Exported(TContent part, ExportContentContext context) { }

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

        protected virtual void Cloning(TContent originalPart, TContent clonePart, CloneContentContext context) { }

        protected virtual void Cloned(TContent originalPart, TContent clonePart, CloneContentContext context) { }

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

                if (shape == null) {
                    return null;
                }

                return AddAlternates(shape, ctx); ;
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