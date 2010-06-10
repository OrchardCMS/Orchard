using System.Collections.Generic;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;

namespace Orchard.ContentManagement.Drivers {

    public interface IContentPartDriver : IEvents {
        DriverResult BuildDisplayModel(BuildDisplayModelContext context);
        DriverResult BuildEditorModel(BuildEditorModelContext context);
        DriverResult UpdateEditorModel(UpdateEditorModelContext context);

        IEnumerable<ContentPartInfo> GetPartInfo();
    }

    public abstract class ContentPartDriver<TContent> : IContentPartDriver where TContent : ContentPart, new() {
        protected virtual string Prefix { get { return ""; } }
        protected virtual string Zone { get { return "body"; } }

        DriverResult IContentPartDriver.BuildDisplayModel(BuildDisplayModelContext context) {
            var part = context.ContentItem.As<TContent>();
            return part == null ? null : Display(part, context.DisplayType);
        }

        DriverResult IContentPartDriver.BuildEditorModel(BuildEditorModelContext context) {
            var part = context.ContentItem.As<TContent>();
            return part == null ? null : Editor(part);
        }

        DriverResult IContentPartDriver.UpdateEditorModel(UpdateEditorModelContext context) {
            var part = context.ContentItem.As<TContent>();
            return part == null ? null : Editor(part, context.Updater);
        }

        protected virtual DriverResult Display(TContent part, string displayType) { return null; }
        protected virtual DriverResult Editor(TContent part) { return null; }
        protected virtual DriverResult Editor(TContent part, IUpdateModel updater) { return null; }


        public ContentPartTemplateResult ContentPartTemplate(object model) {
            return new ContentPartTemplateResult(model, null, Prefix).Location(Zone);
        }

        public ContentPartTemplateResult ContentPartTemplate(object model, string template) {
            return new ContentPartTemplateResult(model, template, Prefix).Location(Zone);
        }

        public ContentPartTemplateResult ContentPartTemplate(object model, string template, string prefix) {
            return new ContentPartTemplateResult(model, template, prefix).Location(Zone);
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