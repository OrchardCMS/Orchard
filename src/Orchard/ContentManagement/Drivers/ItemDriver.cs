using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.ContentManagement.Drivers {
    public interface IItemDriver : IEvents {
        IEnumerable<ContentType> GetContentTypes();
        void GetItemMetadata(GetItemMetadataContext context);

        DriverResult BuildDisplayModel(BuildDisplayModelContext context);
        DriverResult BuildEditorModel(BuildEditorModelContext context);
        DriverResult UpdateEditorModel(UpdateEditorModelContext context);
    }

    public abstract class ItemDriver<TContent> : PartDriver<TContent>, IItemDriver where TContent : class, IContent {
        private ContentType _contentType;

        public ItemDriver() {
        }

        public ItemDriver(ContentType contentType) {
            _contentType = contentType;
        }

        IEnumerable<ContentType> IItemDriver.GetContentTypes() {
            var contentType = GetContentType();
            return contentType != null ? new[] { contentType } : Enumerable.Empty<ContentType>();
        }

        void IItemDriver.GetItemMetadata(GetItemMetadataContext context) {
            var item = context.ContentItem.As<TContent>();
            if (item != null) {
                context.Metadata.DisplayText = GetDisplayText(item) ?? context.Metadata.DisplayText;
                context.Metadata.DisplayRouteValues = GetDisplayRouteValues(item) ?? context.Metadata.DisplayRouteValues;
                context.Metadata.EditorRouteValues = GetEditorRouteValues(item) ?? context.Metadata.EditorRouteValues;
            }
        }

        DriverResult IItemDriver.BuildDisplayModel(BuildDisplayModelContext context) {
            var part = context.ContentItem.As<TContent>();
            if (part == null) {
                return null;
            }
            if (context.DisplayModel.GetType() != typeof(ItemDisplayModel<TContent>)) {
                return Display(new ItemDisplayModel<TContent>(context.DisplayModel), context.DisplayType);
            }
            return Display((ItemDisplayModel<TContent>)context.DisplayModel, context.DisplayType);
        }

        DriverResult IItemDriver.BuildEditorModel(BuildEditorModelContext context) {
            var part = context.ContentItem.As<TContent>();
            if (part == null) {
                return null;
            }
            if (context.EditorModel.GetType() != typeof(ItemEditorModel<TContent>)) {
                return Editor(new ItemEditorModel<TContent>(context.EditorModel));
            }
            return Editor((ItemEditorModel<TContent>)context.EditorModel);
        }

        DriverResult IItemDriver.UpdateEditorModel(UpdateEditorModelContext context) {
            var part = context.ContentItem.As<TContent>();
            if (part == null) {
                return null;
            }
            if (context.EditorModel.GetType() != typeof(ItemEditorModel<TContent>)) {
                return Editor(new ItemEditorModel<TContent>(context.EditorModel), context.Updater);
            }
            return Editor((ItemEditorModel<TContent>)context.EditorModel, context.Updater);
        }

        protected virtual ContentType GetContentType() { return _contentType; }
        protected virtual string GetDisplayText(TContent item) { return null; }
        protected virtual RouteValueDictionary GetDisplayRouteValues(TContent item) { return null; }
        protected virtual RouteValueDictionary GetEditorRouteValues(TContent item) { return null; }

        protected virtual DriverResult Display(ItemDisplayModel<TContent> displayModel, string displayType) { return null; }
        protected virtual DriverResult Editor(ItemEditorModel<TContent> editorModel) { return null; }
        protected virtual DriverResult Editor(ItemEditorModel<TContent> editorModel, IUpdateModel updater) { return null; }

        public ItemTemplateResult<TContent> ItemTemplate(string templateName) {
            return new ItemTemplateResult<TContent>(templateName);
        }

    }
}
