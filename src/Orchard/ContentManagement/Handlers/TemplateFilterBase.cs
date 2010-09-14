namespace Orchard.ContentManagement.Handlers {
    public abstract class TemplateFilterBase<TPart> : IContentTemplateFilter where TPart : class, IContent {
        protected virtual void GetContentItemMetadata(GetContentItemMetadataContext context, TPart instance) { }
        protected virtual void BuildDisplayShape(BuildDisplayModelContext context, TPart instance) { }
        protected virtual void BuildEditorShape(BuildEditorModelContext context, TPart instance) { }
        protected virtual void UpdateEditorShape(UpdateEditorModelContext context, TPart instance) { }

        void IContentTemplateFilter.GetContentItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.Is<TPart>())
                GetContentItemMetadata(context, context.ContentItem.As<TPart>());
        }

        void IContentTemplateFilter.BuildDisplayShape(BuildDisplayModelContext context) {
            if (context.ContentItem.Is<TPart>())
                BuildDisplayShape(context, context.ContentItem.As<TPart>());
        }

        void IContentTemplateFilter.BuildEditorShape(BuildEditorModelContext context) {
            if (context.ContentItem.Is<TPart>())
                BuildEditorShape(context, context.ContentItem.As<TPart>());
        }

        void IContentTemplateFilter.UpdateEditorShape(UpdateEditorModelContext context) {
            if (context.ContentItem.Is<TPart>())
                UpdateEditorShape(context, context.ContentItem.As<TPart>());
        }
    }
}
