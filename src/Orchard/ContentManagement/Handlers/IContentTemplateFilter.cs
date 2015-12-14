namespace Orchard.ContentManagement.Handlers {
    interface IContentTemplateFilter : IContentFilter {
        void GetContentItemMetadata(GetContentItemMetadataContext context);
        void BuildDisplayShape(BuildDisplayContext context);
        void BuildEditorShape(BuildEditorContext context);
        void UpdateEditorShape(UpdateEditorContext context);
    }
}
