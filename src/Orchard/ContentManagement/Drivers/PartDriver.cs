using System;
using Orchard.ContentManagement.Handlers;

namespace Orchard.ContentManagement.Drivers {
    public interface IPartDriver : IEvents {
        DriverResult BuildDisplayModel(BuildDisplayModelContext context);
        DriverResult BuildEditorModel(BuildEditorModelContext context);
        DriverResult UpdateEditorModel(UpdateEditorModelContext context);
    }

    public abstract class PartDriver<TPart> : IPartDriver where TPart : class, IContent {

        protected virtual string Prefix { get { return ""; } }
        protected virtual string Zone { get { return "body"; } }

        DriverResult IPartDriver.BuildDisplayModel(BuildDisplayModelContext context) {
            var part = context.ContentItem.As<TPart>();
            return part == null ? null : Display(part, context.DisplayType);
        }

        DriverResult IPartDriver.BuildEditorModel(BuildEditorModelContext context) {
            var part = context.ContentItem.As<TPart>();
            return part == null ? null : Editor(part);
        }

        DriverResult IPartDriver.UpdateEditorModel(UpdateEditorModelContext context) {
            var part = context.ContentItem.As<TPart>();
            return part == null ? null : Editor(part, context.Updater);
        }

        protected virtual DriverResult Display(TPart part, string displayType) {return null;}
        protected virtual DriverResult Editor(TPart part) {return null;}
        protected virtual DriverResult Editor(TPart part, IUpdateModel updater) {return null;}


        public TemplateResult PartialView(object model) {
            return new TemplateResult(model, null, Prefix).Location(Zone);
        }
        public TemplateResult PartialView(object model, string template) {
            return new TemplateResult(model, template, Prefix).Location(Zone);
        }
        public TemplateResult PartialView(object model, string template, string prefix) {
            return new TemplateResult(model, template, prefix).Location(Zone);
        }
        public CombinedResult Combined(params DriverResult[] results) {
            return new CombinedResult(results);
        }
    }
}