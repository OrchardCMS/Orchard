using System;
using Orchard.ContentManagement.Handlers;

namespace Orchard.ContentManagement.Drivers {
    public interface IPartDriver : IEvents {
        DriverResult BuildDisplayModel(BuildDisplayModelContext context);
        DriverResult BuildEditorModel(BuildEditorModelContext context);
        DriverResult UpdateEditorModel(UpdateEditorModelContext context);
    }

    public abstract class PartDriver<TContent> : IPartDriver where TContent : class, IContent {

        protected virtual string Prefix { get { return ""; } }
        protected virtual string Zone { get { return "body"; } }

        DriverResult IPartDriver.BuildDisplayModel(BuildDisplayModelContext context) {
            var part = context.ContentItem.As<TContent>();
            return part == null ? null : Display(part, context.DisplayType);
        }

        DriverResult IPartDriver.BuildEditorModel(BuildEditorModelContext context) {
            var part = context.ContentItem.As<TContent>();
            return part == null ? null : Editor(part);
        }

        DriverResult IPartDriver.UpdateEditorModel(UpdateEditorModelContext context) {
            var part = context.ContentItem.As<TContent>();
            return part == null ? null : Editor(part, context.Updater);
        }

        protected virtual DriverResult Display(TContent part, string displayType) {return null;}
        protected virtual DriverResult Editor(TContent part) {return null;}
        protected virtual DriverResult Editor(TContent part, IUpdateModel updater) {return null;}


        public PartTemplateResult PartTemplate(object model) {
            return new PartTemplateResult(model, null, Prefix).Location(Zone);
        }
        public PartTemplateResult PartTemplate(object model, string template) {
            return new PartTemplateResult(model, template, Prefix).Location(Zone);
        }
        public PartTemplateResult PartTemplate(object model, string template, string prefix) {
            return new PartTemplateResult(model, template, prefix).Location(Zone);
        }
        public CombinedResult Combined(params DriverResult[] results) {
            return new CombinedResult(results);
        }
    }
}