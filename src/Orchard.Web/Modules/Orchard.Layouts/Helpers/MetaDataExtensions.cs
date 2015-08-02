using Orchard.ContentManagement.MetaData.Builders;

namespace Orchard.Layouts.Helpers {
    public static class MetaDataExtensions {
        public static ContentPartDefinitionBuilder Placeable(this ContentPartDefinitionBuilder builder, bool placeable = true) {
            return builder.WithSetting("ContentPartLayoutSettings.Placeable", placeable.ToString());
        }
    }
}
