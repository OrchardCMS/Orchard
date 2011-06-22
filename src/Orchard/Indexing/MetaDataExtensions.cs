using Orchard.ContentManagement.Metadata.Builders;

namespace Orchard.Indexing {
    public static class MetaDataExtensions {
        public static ContentTypeDefinitionBuilder Indexed(this ContentTypeDefinitionBuilder builder) {
            return builder.WithSetting("TypeIndexing.Included", "true");
        }
    }
}
