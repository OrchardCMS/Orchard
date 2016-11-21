using System.Globalization;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Environment.Extensions;

namespace Orchard.Localization.Extensions {
    [OrchardFeature("Orchard.Localization.CultureNeutralPartsAndFields")]
    public static class MetaDataExtensions {
        public static ContentPartFieldDefinitionBuilder CultureNeutral(this ContentPartFieldDefinitionBuilder builder, bool cultureNeutral = true) {
            return builder.WithSetting("LocalizationCultureNeutralitySettings.CultureNeutral", cultureNeutral.ToString(CultureInfo.InvariantCulture));
        }
        public static ContentTypePartDefinitionBuilder CultureNeutral(this ContentTypePartDefinitionBuilder builder, bool cultureNeutral = true) {
            return builder.WithSetting("LocalizationCultureNeutralitySettings.CultureNeutral", cultureNeutral.ToString(CultureInfo.InvariantCulture));
        }
    }
}