using System.Globalization;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Environment.Extensions;

namespace Orchard.Localization.Settings {
    [OrchardFeature("Orchard.Localization.CultureNeutralPartsAndFields")]
    public class LocalizationCultureNeutralitySettings {
        public bool CultureNeutral { get; set; }

        public void BuildSettings(ContentPartFieldDefinitionBuilder builder) {
            builder.WithSetting("LocalizationCultureNeutralitySettings.CultureNeutral", CultureNeutral.ToString(CultureInfo.InvariantCulture));
        }
        public void BuildSettings(ContentTypePartDefinitionBuilder builder) {
            builder.WithSetting("LocalizationCultureNeutralitySettings.CultureNeutral", CultureNeutral.ToString(CultureInfo.InvariantCulture));
        }
    }
}