using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.Drivers.FieldStorage;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.Drivers {
    public abstract class ContentFieldDriver<TField> : IContentFieldDriver where TField : ContentField, new() {
        protected virtual string Prefix { get { return ""; } }
        protected virtual string Zone { get { return "body"; } }

        DriverResult IContentFieldDriver.BuildDisplayModel(BuildDisplayModelContext context) {
            var results = context.ContentItem.Parts.SelectMany(part => part.Fields).
                OfType<TField>().
                Select(field => Display(field, context.DisplayType));
            return Combined(results.ToArray());
        }

        DriverResult IContentFieldDriver.BuildEditorModel(BuildEditorModelContext context) {
            var results = context.ContentItem.Parts.SelectMany(part => part.Fields).
                OfType<TField>().
                Select(field => Editor(field));
            return Combined(results.ToArray());
        }

        DriverResult IContentFieldDriver.UpdateEditorModel(UpdateEditorModelContext context) {
            var results = context.ContentItem.Parts.SelectMany(part => part.Fields).
                OfType<TField>().
                Select(field => Editor(field, context.Updater));
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


        protected virtual DriverResult Display(TField field, string displayType) { return null; }
        protected virtual DriverResult Editor(TField field) { return null; }
        protected virtual DriverResult Editor(TField field, IUpdateModel updater) { return null; }


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