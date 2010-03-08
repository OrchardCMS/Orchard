using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Orchard.ContentManagement.Handlers;
using Orchard.Mvc.ViewModels;

namespace Orchard.ContentManagement.Drivers {
    public abstract class ContentItemDriver<TContent> : ContentPartDriver<TContent>, IContentItemDriver where TContent : class, IContent {
        private readonly ContentType _contentType;
        protected virtual bool UseDefaultTemplate { get { return false; } }

        public ContentItemDriver() {
        }

        public ContentItemDriver(ContentType contentType) {
            _contentType = contentType;
        }

        IEnumerable<ContentType> IContentItemDriver.GetContentTypes() {
            var contentType = GetContentType();
            return contentType != null ? new[] { contentType } : Enumerable.Empty<ContentType>();
        }

        void IContentItemDriver.GetContentItemMetadata(GetContentItemMetadataContext context) {
            var item = context.ContentItem.As<TContent>();
            if (item != null) {
                context.Metadata.DisplayText = GetDisplayText(item) ?? context.Metadata.DisplayText;
                context.Metadata.DisplayRouteValues = GetDisplayRouteValues(item) ?? context.Metadata.DisplayRouteValues;
                context.Metadata.EditorRouteValues = GetEditorRouteValues(item) ?? context.Metadata.EditorRouteValues;
            }
        }

        DriverResult IContentItemDriver.BuildDisplayModel(BuildDisplayModelContext context) {
            var part = context.ContentItem.As<TContent>();
            if (part == null) {
                return null;
            }
            if (context.ViewModel.GetType() != typeof(ContentItemViewModel<TContent>)) {
                return Display(new ContentItemViewModel<TContent>(context.ViewModel), context.DisplayType);
            }
            return Display((ContentItemViewModel<TContent>)context.ViewModel, context.DisplayType);
        }

        DriverResult IContentItemDriver.BuildEditorModel(BuildEditorModelContext context) {
            var part = context.ContentItem.As<TContent>();
            if (part == null) {
                return null;
            }
            if (context.ViewModel.GetType() != typeof(ContentItemViewModel<TContent>)) {
                return Editor(new ContentItemViewModel<TContent>(context.ViewModel));
            }
            return Editor((ContentItemViewModel<TContent>)context.ViewModel);
        }

        DriverResult IContentItemDriver.UpdateEditorModel(UpdateEditorModelContext context) {
            var part = context.ContentItem.As<TContent>();
            if (part == null) {
                return null;
            }
            if (context.ViewModel.GetType() != typeof(ContentItemViewModel<TContent>)) {
                return Editor(new ContentItemViewModel<TContent>(context.ViewModel), context.Updater);
            }
            return Editor((ContentItemViewModel<TContent>)context.ViewModel, context.Updater);
        }

        protected virtual ContentType GetContentType() { return _contentType; }
        protected virtual string GetDisplayText(TContent item) { return null; }
        public virtual RouteValueDictionary GetDisplayRouteValues(TContent item) { return null; }
        public virtual RouteValueDictionary GetEditorRouteValues(TContent item) { return null; }

        protected virtual DriverResult Display(ContentItemViewModel<TContent> viewModel, string displayType) { return GetDefaultItemTemplate(); }
        protected virtual DriverResult Editor(ContentItemViewModel<TContent> viewModel) { return GetDefaultItemTemplate(); }
        protected virtual DriverResult Editor(ContentItemViewModel<TContent> viewModel, IUpdateModel updater) { return GetDefaultItemTemplate(); }

        public ContentItemTemplateResult<TContent> ContentItemTemplate(string templateName) {
            return new ContentItemTemplateResult<TContent>(templateName);
        }

        private DriverResult GetDefaultItemTemplate() {
            return UseDefaultTemplate
                ? ContentItemTemplate("Items/ContentItem")
                : null;
        }
    }
}