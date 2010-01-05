namespace Orchard.ContentManagement.Handlers {
    public abstract class TemplateFilterBase<TPart> : IContentTemplateFilter where TPart : class, IContent {
        protected virtual void GetContentItemMetadata(GetContentItemMetadataContext context, TPart instance) { }
        protected virtual void BuildDisplayModel(BuildDisplayModelContext context, TPart instance) { }
        protected virtual void BuildEditorModel(BuildEditorModelContext context, TPart instance) { }
        protected virtual void UpdateEditorModel(UpdateEditorModelContext context, TPart instance) { }

        void IContentTemplateFilter.GetContentItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.Is<TPart>())
                GetContentItemMetadata(context, context.ContentItem.As<TPart>());
        }

        void IContentTemplateFilter.BuildDisplayModel(BuildDisplayModelContext context) {
            if (context.ContentItem.Is<TPart>())
                BuildDisplayModel(context, context.ContentItem.As<TPart>());
        }

        void IContentTemplateFilter.BuildEditorModel(BuildEditorModelContext context) {
            if (context.ContentItem.Is<TPart>())
                BuildEditorModel(context, context.ContentItem.As<TPart>());
        }

        void IContentTemplateFilter.UpdateEditorModel(UpdateEditorModelContext context) {
            if (context.ContentItem.Is<TPart>())
                UpdateEditorModel(context, context.ContentItem.As<TPart>());
        }
    }
}
