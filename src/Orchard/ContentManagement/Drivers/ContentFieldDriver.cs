using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;
using Orchard.Logging;

namespace Orchard.ContentManagement.Drivers {
    public abstract class ContentFieldDriver<TField> : IContentFieldDriver where TField : ContentField, new() {
        protected virtual string Prefix { get { return ""; } }
        protected virtual string Zone { get { return "Content"; } }

        void IContentFieldDriver.GetContentItemMetadata(GetContentItemMetadataContext context) {
            Process(context.ContentItem, (part, field) => GetContentItemMetadata(part, field, context.Metadata), context.Logger);
        }

        DriverResult IContentFieldDriver.BuildDisplayShape(BuildDisplayContext context) {
            return Process(context.ContentItem, (part, field) => {
                DriverResult result = Display(part, field, context.DisplayType, context.New);
                
                if (result != null) {
                    result.ContentPart = part;
                    result.ContentField = field;
                }
                
                return result;
            }, context.Logger);
        }

        DriverResult IContentFieldDriver.BuildEditorShape(BuildEditorContext context) {
            return Process(context.ContentItem, (part, field) => {
                DriverResult result =  Editor(part, field, context.New);
                
                if (result != null) {
                    result.ContentPart = part;
                    result.ContentField = field;
                }
                
                return result;
            }, context.Logger);
        }

        DriverResult IContentFieldDriver.UpdateEditorShape(UpdateEditorContext context) {
            return Process(context.ContentItem, (part, field) => {
                // Checking if the editor needs to be updated (e.g. if any of the shapes were not hidden).
                DriverResult editor = Editor(part, field, context.New);
                IEnumerable<ContentShapeResult> contentShapeResults = editor.GetShapeResults();
                
                if (contentShapeResults.Any(contentShapeResult =>
                    contentShapeResult == null || contentShapeResult.WasDisplayed(context))) {
                    DriverResult result = Editor(part, field, context.Updater, context.New);

                    if (result != null) {
                        result.ContentPart = part;
                        result.ContentField = field;
                    }

                    return result;
                }

                return editor;
            }, context.Logger);
        }

        void IContentFieldDriver.Importing(ImportContentContext context) {
            Process(context.ContentItem, (part, field) => Importing(part, field, context), context.Logger);
        }

        void IContentFieldDriver.Imported(ImportContentContext context) {
            Process(context.ContentItem, (part, field) => Imported(part, field, context), context.Logger);
        }

        void IContentFieldDriver.ImportCompleted(ImportContentContext context) {
            Process(context.ContentItem, (part, field) => ImportCompleted(part, field, context), context.Logger);
        }

        void IContentFieldDriver.Exporting(ExportContentContext context) {
            Process(context.ContentItem, (part, field) => Exporting(part, field, context), context.Logger);
        }

        void IContentFieldDriver.Exported(ExportContentContext context) {
            Process(context.ContentItem, (part, field) => Exported(part, field, context), context.Logger);
        }

        void IContentFieldDriver.Describe(DescribeMembersContext context) {
            Describe(context);
        }

        void Process(ContentItem item, Action<ContentPart, TField> effort, ILogger logger) {
            var occurences = item.Parts.SelectMany(part => part.Fields.OfType<TField>().Select(field => new { part, field }));
            occurences.Invoke(pf => effort(pf.part, pf.field), logger);
        }

        DriverResult Process(ContentItem item, Func<ContentPart, TField, DriverResult> effort, ILogger logger) {
            var results = item.Parts
                .SelectMany(part => part.Fields.OfType<TField>().Select(field => new { part, field }))
                .Invoke(pf => effort(pf.part, pf.field), logger);

            return Combined(results.ToArray());
        }

        public IEnumerable<ContentFieldInfo> GetFieldInfo() {
            var contentFieldInfo = new[] {
                new ContentFieldInfo {
                    FieldTypeName = typeof (TField).Name,
                    Factory = (partFieldDefinition, storage) => new TField {
                        PartFieldDefinition = partFieldDefinition,
                        Storage = storage,
                    }
                }
            };

            return contentFieldInfo;
        }

        protected virtual void GetContentItemMetadata(ContentPart part, TField field, ContentItemMetadata metadata) { }

        protected virtual DriverResult Display(ContentPart part, TField field, string displayType, dynamic shapeHelper) { return null; }
        protected virtual DriverResult Editor(ContentPart part, TField field, dynamic shapeHelper) { return null; }
        protected virtual DriverResult Editor(ContentPart part, TField field, IUpdateModel updater, dynamic shapeHelper) { return null; }
        
        protected virtual void Importing(ContentPart part, TField field, ImportContentContext context) { }
        protected virtual void Imported(ContentPart part, TField field, ImportContentContext context) { }
        protected virtual void ImportCompleted(ContentPart part, TField field, ImportContentContext context) { }
        protected virtual void Exporting(ContentPart part, TField field, ExportContentContext context) { }
        protected virtual void Exported(ContentPart part, TField field, ExportContentContext context) { }

        protected virtual void Describe(DescribeMembersContext context) { }

        public ContentShapeResult ContentShape(string shapeType, Func<dynamic> factory) {
            return ContentShapeImplementation(shapeType, null, ctx => factory());
        }

        public ContentShapeResult ContentShape(string shapeType, string differentiator, Func<dynamic> factory) {
            return ContentShapeImplementation(shapeType, differentiator, ctx => factory());
        }

        public ContentShapeResult ContentShape(string shapeType, Func<dynamic, dynamic> factory) {
            return ContentShapeImplementation(shapeType, null, ctx => factory(CreateShape(ctx, shapeType)));
        }

        public ContentShapeResult ContentShape(string shapeType, string differentiator, Func<dynamic, dynamic> factory) {
            return ContentShapeImplementation(shapeType, differentiator, ctx => factory(CreateShape(ctx, shapeType)));
        }

        private ContentShapeResult ContentShapeImplementation(string shapeType, string differentiator, Func<BuildShapeContext, object> shapeBuilder) {
            return new ContentShapeResult(shapeType, Prefix, ctx => AddAlternates(shapeBuilder(ctx), ctx, differentiator)).Differentiator(differentiator);
        }

        private static object AddAlternates(dynamic shape, BuildShapeContext ctx, string differentiator) {
            // automatically add shape alternates for shapes added by fields
            // for fields on dynamic parts the part name is the same as the content type name

            ShapeMetadata metadata = shape.Metadata;

            // if no ContentItem property has been set, assign it
            if (shape.ContentItem == null) {
                shape.ContentItem = ctx.ContentItem;
            }

            var shapeType = metadata.Type;
            var fieldName = differentiator ?? String.Empty;
            var partName = ctx.ContentPart.PartDefinition.Name;
            string contentType = shape.ContentItem.ContentType;

            // whether the content type has been created dynamically or not
            var dynamicType = String.Equals(partName, contentType, StringComparison.Ordinal);

            // [ShapeType__FieldName] e.g. Fields/Common.Text-Teaser
            if ( !String.IsNullOrEmpty(fieldName) )
                metadata.Alternates.Add(shapeType + "__" + EncodeAlternateElement(fieldName));

            // [ShapeType__PartName] e.g. Fields/Common.Text-TeaserPart
            if ( !String.IsNullOrEmpty(partName) ) {
                metadata.Alternates.Add(shapeType + "__" + EncodeAlternateElement(partName));
            }

            // [ShapeType]__[ContentType]__[PartName] e.g. Fields/Common.Text-Blog-TeaserPart
            if ( !String.IsNullOrEmpty(partName) && !String.IsNullOrEmpty(contentType) && !dynamicType ) {
                metadata.Alternates.Add(EncodeAlternateElement(shapeType + "__" + contentType + "__" + partName));
            }

            // [ShapeType]__[PartName]__[FieldName] e.g. Fields/Common.Text-TeaserPart-Teaser
            if ( !String.IsNullOrEmpty(partName) && !String.IsNullOrEmpty(fieldName) ) {
                metadata.Alternates.Add(EncodeAlternateElement(shapeType + "__" + partName + "__" + fieldName));
            }

            // [ShapeType]__[ContentType]__[FieldName] e.g. Fields/Common.Text-Blog-Teaser
            if ( !String.IsNullOrEmpty(contentType) && !String.IsNullOrEmpty(fieldName) ) {
                metadata.Alternates.Add(EncodeAlternateElement(shapeType + "__" + contentType + "__" + fieldName));
            }

            // [ShapeType]__[ContentType]__[PartName]__[FieldName] e.g. Fields/Common.Text-Blog-TeaserPart-Teaser
            if ( !String.IsNullOrEmpty(contentType) && !String.IsNullOrEmpty(partName) && !String.IsNullOrEmpty(fieldName) && !dynamicType ) {
                metadata.Alternates.Add(EncodeAlternateElement(shapeType + "__" + contentType + "__" + partName + "__" + fieldName));
            }
            
            return shape;
        }

        private static object CreateShape(BuildShapeContext context, string shapeType) {
            IShapeFactory shapeFactory = context.New;
            return shapeFactory.Create(shapeType);
        }

        [Obsolete]
        public ContentTemplateResult ContentFieldTemplate(object model) {
            return new ContentTemplateResult(model, null, Prefix).Location(Zone);
        }
        [Obsolete]
        public ContentTemplateResult ContentFieldTemplate(object model, string template) {
            return new ContentTemplateResult(model, template, Prefix).Location(Zone);
        }
        [Obsolete]
        public ContentTemplateResult ContentFieldTemplate(object model, string template, string prefix) {
            return new ContentTemplateResult(model, template, prefix).Location(Zone);
        }

        public CombinedResult Combined(params DriverResult[] results) {
            return new CombinedResult(results);
        }

        /// <summary>
        /// Encodes dashed and dots so that they don't conflict in filenames 
        /// </summary>
        /// <param name="alternateElement"></param>
        /// <returns></returns>
        private static string EncodeAlternateElement(string alternateElement) {
            return alternateElement.Replace("-", "__").Replace(".", "_");
        }
    }
}