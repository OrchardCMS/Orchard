namespace Orchard.ContentManagement.Handlers {
    interface IContentTemplateFilter : IContentFilter {
        void GetContentItemMetadata(GetContentItemMetadataContext context);
        void BuildDisplayShape(BuildDisplayModelContext context);
        void BuildEditorShape(BuildEditorModelContext context);
        void UpdateEditorShape(UpdateEditorModelContext context);
    }
}
