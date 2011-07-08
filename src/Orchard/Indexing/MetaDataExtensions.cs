using Orchard.ContentManagement.Definition.Builders;

namespace Orchard.Indexing {
    public static class MetadataExtensions {
        public static ContentTypeDefinitionBuilder Indexed(this ContentTypeDefinitionBuilder builder) {
            return builder.WithSetting("TypeIndexing.Included", "true");
        }
    }
}
