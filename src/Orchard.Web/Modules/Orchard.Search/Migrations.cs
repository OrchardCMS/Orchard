using System.Linq;
using Orchard.ContentManagement.MetaData;
using Orchard.Data;
using Orchard.Data.Migration;
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
}