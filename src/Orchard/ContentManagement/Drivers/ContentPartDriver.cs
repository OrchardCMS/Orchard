using System;
using System.Collections.Generic;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;

namespace Orchard.ContentManagement.Drivers {
    public abstract class ContentPartDriver<TContent> : IContentPartDriver where TContent : ContentPart, new() {
        protected virtual string Prefix { get { return ""; } }
        protected virtual string Zone { get { return "Primary"; } }

        DriverResult IContentPartDriver.BuildDisplay(BuildDisplayContext context) {
            var part = context.ContentItem.As<TContent>();
            return part == null ? null : Display(part, context.DisplayType, context.New);
        }

        DriverResult IContentPartDriver.BuildEditor(BuildEditorContext context) {
            var part = context.ContentItem.As<TContent>();
            return part == null ? null : Editor(part, context.New);
        }

        DriverResult IContentPartDriver.UpdateEditor(UpdateEditorContext context) {
            var part = context.ContentItem.As<TContent>();
            return part == null ? null : Editor(part, context.Updater, context.New);
        }

        protected virtual DriverResult Display(TContent part, string displayType, dynamic shapeHelper) { return null; }
        protected virtual DriverResult Editor(TContent part, dynamic shapeHelper) { return null; }
        protected virtual DriverResult Editor(TContent part, IUpdateModel updater, dynamic shapeHelper) { return null; }

        public ContentShapeResult ContentShape(dynamic shape) {
            return new ContentShapeResult(shape, Prefix).Location(Zone);
        }

        [Obsolete]
        public ContentTemplateResult ContentPartTemplate(object model) {
            return new ContentTemplateResult(model, null, Prefix).Location(Zone);
        }
        [Obsolete]
        public ContentTemplateResult ContentPartTemplate(object model, string template) {
            return new ContentTemplateResult(model, template, Prefix).Location(Zone);
        }
        [Obsolete]
        public ContentTemplateResult ContentPartTemplate(object model, string template, string prefix) {
            return new ContentTemplateResult(model, template, prefix).Location(Zone);
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