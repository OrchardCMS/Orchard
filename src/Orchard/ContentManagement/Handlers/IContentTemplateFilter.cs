namespace Orchard.ContentManagement.Handlers {
    interface IContentTemplateFilter : IContentFilter {
        void GetContentItemMetadata(GetContentItemMetadataContext context);
        void BuildDisplayModel(BuildDisplayModelContext context);
        void BuildEditorModel(BuildEditorModelContext context);
        void UpdateEditorModel(UpdateEditorModelContext context);
    }
}
