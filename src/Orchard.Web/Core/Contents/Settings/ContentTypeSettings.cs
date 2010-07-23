using Orchard.ContentManagement.MetaData.Builders;

namespace Orchard.Core.Contents.Settings {
    public class ContentTypeSettings {
        /// <summary>
        /// This setting is used to display a Content Type in Content Mamagement menu like
        /// </summary>
        public bool Creatable { get; set; }
    }

    public static class MetaDataExtensions {
        public static ContentTypeDefinitionBuilder Creatable(this ContentTypeDefinitionBuilder builder, bool creatable = true) {
            return builder.WithSetting(typeof(ContentTypeSettings).Name + ".Creatable", creatable.ToString());
        }
    }
}