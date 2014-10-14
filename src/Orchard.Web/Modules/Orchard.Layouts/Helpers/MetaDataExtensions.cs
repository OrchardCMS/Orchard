using Orchard.ContentManagement.MetaData.Builders;

namespace Orchard.Layouts.Helpers {
    public static class MetaDataExtensions {
        public static ContentPartDefinitionBuilder Placable(this ContentPartDefinitionBuilder builder, bool placable = true) {
            return builder.WithSetting("ContentPartLayoutSettings.Placable", placable.ToString());
        }
    }
}
