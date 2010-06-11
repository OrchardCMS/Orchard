using Orchard.ContentManagement.Handlers;

namespace Orchard.ContentManagement.Drivers {

    public interface IContentFieldDriver : IEvents {
        DriverResult BuildDisplayModel(BuildDisplayModelContext context);
        DriverResult BuildEditorModel(BuildEditorModelContext context);
        DriverResult UpdateEditorModel(UpdateEditorModelContext context);
    }

    public abstract class ContentFieldDriver<TContent> : IContentFieldDriver where TContent : ContentField, new() {
        protected virtual string Prefix { get { return ""; } }
        protected virtual string Zone { get { return "body"; } }

        DriverResult IContentFieldDriver.BuildDisplayModel(BuildDisplayModelContext context) {
            var field = context.ContentItem.As<TContent>();
            return field == null ? null : Display(field, context.DisplayType);
        }

        DriverResult IContentFieldDriver.BuildEditorModel(BuildEditorModelContext context) {
            var field = context.ContentItem.As<TContent>();
            return field == null ? null : Editor(field);
        }

        DriverResult IContentFieldDriver.UpdateEditorModel(UpdateEditorModelContext context) {
            var field = context.ContentItem.As<TContent>();
            return field == null ? null : Editor(field, context.Updater);
        }

        protected virtual DriverResult Display(TContent field, string displayType) { return null; }
        protected virtual DriverResult Editor(TContent field) { return null; }
        protected virtual DriverResult Editor(TContent field, IUpdateModel updater) { return null; }


        public ContentFieldTemplateResult ContentPartTemplate(object model) {
            return new ContentFieldTemplateResult(model, null, Prefix).Location(Zone);
        }

        public ContentFieldTemplateResult ContentPartTemplate(object model, string template) {
            return new ContentFieldTemplateResult(model, template, Prefix).Location(Zone);
        }

        public ContentFieldTemplateResult ContentPartTemplate(object model, string template, string prefix) {
            return new ContentFieldTemplateResult(model, template, prefix).Location(Zone);
        }

        public CombinedResult Combined(params DriverResult[] results) {
            return new CombinedResult(results);
        }
    }
}