using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using Orchard.Indexing;

namespace Orchard.Search {
    public class SearchDataMigration : DataMigrationImpl {

        public int Create() {

            ContentDefinitionManager.AlterTypeDefinition("SearchForm",
                cfg => cfg
                    .WithPart("SearchFormPart")
                    .WithPart("CommonPart")
                    .WithPart("WidgetPart")
                    .WithSetting("Stereotype", "Widget")
                );

            return 2;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("SearchSettingsPartRecord", table => table
                .AddColumn<string>("SearchIndex", c => c.WithDefault("Search"))
            );

            return 2;
        }
    }

    [OrchardFeature("Orchard.Search.MediaLibrary")]
    public class MediaMigration : DataMigrationImpl {
        private readonly IIndexManager _indexManager;

        public MediaMigration(IIndexManager indexManager) {
            _indexManager = indexManager;
        }

        public int Create() {

            _indexManager.GetSearchIndexProvider().CreateIndex("Media");

            ContentDefinitionManager.AlterTypeDefinition("Image", cfg => cfg.WithSetting("TypeIndexing.Indexes", "Media"));
            ContentDefinitionManager.AlterTypeDefinition("Video", cfg => cfg.WithSetting("TypeIndexing.Indexes", "Media"));
            ContentDefinitionManager.AlterTypeDefinition("Document", cfg => cfg.WithSetting("TypeIndexing.Indexes", "Media"));
            ContentDefinitionManager.AlterTypeDefinition("Audio", cfg => cfg.WithSetting("TypeIndexing.Indexes", "Media"));
            ContentDefinitionManager.AlterTypeDefinition("OEmbed", cfg => cfg.WithSetting("TypeIndexing.Indexes", "Media"));

            return 1;
        }
    }
}