using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.Drivers {
    public abstract class ContentFieldDriver<TField> : IContentFieldDriver where TField : ContentField, new() {
        protected virtual string Prefix { get { return ""; } }
        protected virtual string Zone { get { return "body"; } }

        DriverResult IContentFieldDriver.BuildDisplayModel(BuildDisplayModelContext context) {
            return Process(context.ContentItem, (part, field) => Display(part, field, context.DisplayType));
        }

        DriverResult IContentFieldDriver.BuildEditorModel(BuildEditorModelContext context) {
            return Process(context.ContentItem, (part, field) => Editor(part, field));
        }

        DriverResult IContentFieldDriver.UpdateEditorModel(UpdateEditorModelContext context) {
            return Process(context.ContentItem, (part, field) => Editor(part, field, context.Updater));
        }

        DriverResult Process(ContentItem item, Func<ContentPart, TField, DriverResult> effort) {
            var results = item.Parts
                    .SelectMany(part => part.Fields.OfType<TField>().Select(field => new { part, field }))
                    .Select(pf => effort(pf.part, pf.field));
            return Combined(results.ToArray());
        }

        public IEnumerable<ContentFieldInfo> GetFieldInfo() {
            var contentFieldInfo = new[] {
                new ContentFieldInfo {
                    FieldTypeName = typeof (TField).Name,
                    Factory = (partFieldDefinition, storage) => new TField {
                        PartFieldDefinition = partFieldDefinition,
                        Getter = storage.Getter,
                        Setter = storage.Setter,
                    }
                }
            };

            return contentFieldInfo;
        }


        protected virtual DriverResult Display(ContentPart part, TField field, string displayType) { return null; }
        protected virtual DriverResult Editor(ContentPart part, TField field) { return null; }
        protected virtual DriverResult Editor(ContentPart part, TField field, IUpdateModel updater) { return null; }


        public ContentTemplateResult ContentFieldTemplate(object model) {
            return new ContentTemplateResult(model, null, Prefix).Location(Zone);
        }

        public ContentTemplateResult ContentFieldTemplate(object model, string template) {
            return new ContentTemplateResult(model, template, Prefix).Location(Zone);
        }

        public ContentTemplateResult ContentFieldTemplate(object model, string template, string prefix) {
            return new ContentTemplateResult(model, template, prefix).Location(Zone);
        }

        public CombinedResult Combined(params DriverResult[] results) {
            return new CombinedResult(results);
        }
    }
}