using System.Linq;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using Orchard.Localization.Models;

namespace Orchard.MediaLibrary {

    [OrchardFeature("Orchard.MediaLibrary.LocalizationExtensions")]
    public class MediaLocalizationMigrations : DataMigrationImpl {
        public int Create() {
            var mediaContentTypes = ContentDefinitionManager.ListTypeDefinitions().Where(x => x.Settings.ContainsKey("Stereotype") && x.Settings["Stereotype"].Equals("Media", System.StringComparison.InvariantCultureIgnoreCase));
            // adds LocalizationPart to all "Media" stereotypes
            foreach (var mediaCT in mediaContentTypes) {
                ContentDefinitionManager.AlterTypeDefinition(mediaCT.Name, td => td
                .WithPart("LocalizationPart"));
            }
            return 1;
        }
    }
}