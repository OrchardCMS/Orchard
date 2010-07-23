using Orchard.ContentManagement.MetaData.Builders;

namespace Orchard.Core.Contents.Extensions {
    public static class MetaDataExtensions {
        public static ContentTypeDefinitionBuilder Creatable(this ContentTypeDefinitionBuilder builder, bool creatable = true) {
            return builder.WithSetting("ContentTypeSettings.Creatable", creatable.ToString());
        }
    }
}
