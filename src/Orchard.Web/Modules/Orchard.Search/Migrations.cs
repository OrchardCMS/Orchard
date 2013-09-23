using System.Linq;
using Orchard.ContentManagement.MetaData;
using Orchard.Data;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using Orchard.Indexing;
using Orchard.Search.Models;

namespace Orchard.Search {
    public class SearchDataMigration : DataMigrationImpl {
        private readonly IRepository<SearchSettingsPartRecord> _searchSettingsPartRecordRepository;

        public SearchDataMigration(IRepository<SearchSettingsPartRecord> searchSettingsPartRecordRepository) {
            _searchSettingsPartRecordRepository = searchSettingsPartRecordRepository;
        }

        public int Create() {

            SchemaBuilder.CreateTable("SearchSettingsPartRecord", table => table
                .ContentPartRecord()
                    .Column<bool>("FilterCulture")
                    .Column<string>("SearchedFields", c => c.Unlimited())
                    .Column<string>("SearchIndex")
                );

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
                .AddColumn<string>("SearchIndex")
            );

            var settings = _searchSettingsPartRecordRepository.Table.FirstOrDefault();
            if (settings != null) {
                settings.SearchIndex = "Search";
            }

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