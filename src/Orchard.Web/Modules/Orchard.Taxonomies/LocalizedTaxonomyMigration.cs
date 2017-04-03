using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Orchard.Taxonomies {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class LocalizedTaxonomyMigration : DataMigrationImpl {

        public int Create() {
            ContentDefinitionManager.AlterTypeDefinition("Taxonomy", cfg => cfg
                  .WithPart("LocalizationPart")
                  );
            return 1;
        }
    }
}