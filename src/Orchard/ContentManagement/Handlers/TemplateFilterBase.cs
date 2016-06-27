namespace Orchard.ContentManagement.Handlers {
    public abstract class TemplateFilterBase<TPart> : IContentTemplateFilter where TPart : class, IContent {
        protected virtual void GetContentItemMetadata(GetContentItemMetadataContext context, TPart instance) { }
        protected virtual void BuildDisplayShape(BuildDisplayContext context, TPart instance) { }
        protected virtual void BuildEditorShape(BuildEditorContext context, TPart instance) { }
        protected virtual void UpdateEditorShape(UpdateEditorContext context, TPart instance) { }

        void IContentTemplateFilter.GetContentItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.Is<TPart>())
                GetContentItemMetadata(context, context.ContentItem.As<TPart>());
        }

        void IContentTemplateFilter.BuildDisplayShape(BuildDisplayContext context) {
            if (context.ContentItem != null && context.ContentItem.Is<TPart>())
                BuildDisplayShape(context, context.ContentItem.As<TPart>());
        }

        void IContentTemplateFilter.BuildEditorShape(BuildEditorContext context) {
            if (context.ContentItem.Is<TPart>())
                BuildEditorShape(context, context.ContentItem.As<TPart>());
        }

        void IContentTemplateFilter.UpdateEditorShape(UpdateEditorContext context) {
            if (context.ContentItem.Is<TPart>())
                UpdateEditorShape(context, context.ContentItem.As<TPart>());
        }
    }
}
