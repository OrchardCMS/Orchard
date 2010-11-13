using Orchard.ContentManagement.MetaData.Builders;

namespace Orchard.Core.Contents.Extensions {
    public static class MetaDataExtensions {
        //todo: revisit "creatable" and "attachable", other words by be more fitting
        public static ContentTypeDefinitionBuilder Creatable(this ContentTypeDefinitionBuilder builder, bool creatable = true) {
            return builder.WithSetting("ContentTypeSettings.Creatable", creatable.ToString());
        }
        public static ContentTypeDefinitionBuilder Draftable(this ContentTypeDefinitionBuilder builder, bool draftable = true) {
            return builder.WithSetting("ContentTypeSettings.Draftable", draftable.ToString());
        }
        public static ContentPartDefinitionBuilder Attachable(this ContentPartDefinitionBuilder builder, bool attachable = true) {
            return builder.WithSetting("ContentPartSettings.Attachable", attachable.ToString());
        }
    }
}
