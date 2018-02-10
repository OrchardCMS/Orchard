using System.Globalization;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Environment.Extensions;

namespace Orchard.Localization.Extensions {
    [OrchardFeature("Orchard.Localization.CultureNeutralPartsAndFields")]
    public static class MetaDataExtensions {
        /// <summary>
        /// Sets the ContentField being built as CultureNeutral. This field will then be synchronized across elements of a localization set.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="cultureNeutral"></param>
        /// <returns></returns>
        public static ContentPartFieldDefinitionBuilder CultureNeutral(this ContentPartFieldDefinitionBuilder builder, bool cultureNeutral = true) {
            return builder.WithSetting("LocalizationCultureNeutralitySettings.CultureNeutral", cultureNeutral.ToString(CultureInfo.InvariantCulture));
        }
        /// <summary>
        /// Sets the ContentPart being built as CultureNeutral. This part will then be synchronized across elements of a localization set.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="cultureNeutral"></param>
        /// <returns></returns>
        public static ContentTypePartDefinitionBuilder CultureNeutral(this ContentTypePartDefinitionBuilder builder, bool cultureNeutral = true) {
            return builder.WithSetting("LocalizationCultureNeutralitySettings.CultureNeutral", cultureNeutral.ToString(CultureInfo.InvariantCulture));
        }
    }
}