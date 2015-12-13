using System;
using System.Globalization;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace Orchard.Indexing {
    public class Migrations : DataMigrationImpl {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager) {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create() {
            SchemaBuilder.CreateTable("IndexingTaskRecord",
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("Action")
                    .Column<DateTime>("CreatedUtc")
                    .Column<int>("ContentItemRecord_id")
                );

            return 2;
        }

        public int UpdateFrom1() {

            foreach (var typeDefinition in _contentDefinitionManager.ListTypeDefinitions()) {
                if (typeDefinition.Settings.ContainsKey("TypeIndexing.Included") && Convert.ToBoolean(typeDefinition.Settings["TypeIndexing.Included"], CultureInfo.InvariantCulture)) {
                    typeDefinition.Settings.Remove("TypeIndexing.Included");
                    typeDefinition.Settings["TypeIndexing.Indexes"] = "Search";
                    _contentDefinitionManager.StoreTypeDefinition(typeDefinition);
                }
            }

            return 2;
        }
    }
}