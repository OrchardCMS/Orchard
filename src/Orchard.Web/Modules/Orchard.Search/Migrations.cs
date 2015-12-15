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
                    .WithPart("IdentityPart")
                    .WithSetting("Stereotype", "Widget")
                );

            return 3;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("SearchSettingsPartRecord", table => table
                .AddColumn<string>("SearchIndex", c => c.WithDefault("Search"))
            );

            return 2;
        }

        public int UpdateFrom2() {
            ContentDefinitionManager.AlterTypeDefinition("SearchForm",
                cfg => cfg.WithPart("IdentityPart"));
           
            return 3;
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


    [OrchardFeature("Orchard.Search.Content")]
    public class AdminSearchMigration : DataMigrationImpl {
        private readonly IIndexManager _indexManager;

        public AdminSearchMigration(IIndexManager indexManager) {
            _indexManager = indexManager;
        }

        public int Create() {
            
            return 1;
        }
    }
}